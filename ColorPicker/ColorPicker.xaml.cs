// WPF Color Picker
// Copyright(C) 2020 Stephen Workman (workman.stephen@gmail.com)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MrWorkman.Wpf {

   /// <summary>
   /// Interaction logic for UserControl1.xaml
   /// </summary>
   public partial class ColorPicker : UserControl {
      public const int GridSize = 256;

      private readonly byte[] _pixels = new byte[GridSize * GridSize * 3];
      private int _hue;

      private bool _mousePressed = false;

      private int _selectionRow = 255;
      private int _selectionCol = 0;

      private class Cell {
         public int Row { get; set; }
         public int Column { get; set; }
      }

      public ColorPicker() {
         InitializeComponent();
         InitializeCanvas();
      }

      public EventHandler<ColorSelectionEventArgs> ColorHover { get; set; }
      public EventHandler<ColorSelectionEventArgs> ColorSelect { get; set; }

      public int Hue {
         get => _hue;
         set {
            _hue = value;
            InitializeCanvas();
            TriggerSelectionEvent(this);
         }
      }

      public Color SelectedColor {
         get => GetColorFromSelectionCoords();
         set {
            var colorModel = new ColorModel(value);

            Hue = (int) colorModel.Hue;

            UpdateSelection(
               row:    255 - (int) (colorModel.Brightness * 255.0),
               column: (int) (colorModel.Saturation * 255.0),
               x:      colorModel.Saturation * ActualWidth,
               y:      ActualHeight - colorModel.Brightness * ActualHeight
            );

            TriggerSelectionEvent(this);
         }
      }

      private Point GetBoundedMouseCoords(Point p) {
         double x = p.X, y = p.Y;

         if (x < 0) {
            x = 0;
         }

         if (x >= PickerCanvas.ActualWidth) {
            x = PickerCanvas.ActualWidth - 1;
         }

         if (y < 0) {
            y = 0;
         }

         if (y >= PickerCanvas.ActualHeight) {
            y = PickerCanvas.ActualHeight - 1;
         }

         return new Point(x, y);
      }

      private Color GetColorFromCell(int row, int column) {
         var o = row * GridSize * 3 + column * 3;
         var r = _pixels[o + 0];
         var g = _pixels[o + 1];
         var b = _pixels[o + 2];

         return Color.FromRgb(r, g, b);
      }

      private Color GetColorFromCell(Cell cell) => GetColorFromCell(cell.Row, cell.Column);

      private Color GetColorFromSelectionCoords() => GetColorFromCell(_selectionRow, _selectionCol);

      private void InitializeCanvas() {
         var stride = GridSize * 3;

         for (int row = 0; row < GridSize; row++) {
            for (int column = 0; column < GridSize; column++) {
               var o = (row * GridSize * 3) + (column * 3);

               var saturation = column / (double) GridSize;
               var value = (GridSize - row) / (double) GridSize;

               var color = ColorModel.ComputeColor(Hue, saturation, value);

               _pixels[o + 0] = color.R;
               _pixels[o + 1] = color.G;
               _pixels[o + 2] = color.B;
            }
         }

         var bitmap = new WriteableBitmap(GridSize, GridSize, 96, 96, PixelFormats.Rgb24, null);
         bitmap.WritePixels(new Int32Rect(0, 0, GridSize, GridSize), _pixels, stride, 0);
         PickerCanvas.Source = bitmap;
      }

      private Cell MouseCoordsToCell(double x, double y) {
         var cell = new Cell {
            Row = (int) (y / (PickerCanvas.ActualHeight - 1) * (GridSize - 1)),
            Column = (int) (x / (PickerCanvas.ActualWidth - 1) * (GridSize - 1))
         };

         return cell;
      }

      private Cell MouseCoordsToCell(Point point) => MouseCoordsToCell(point.X, point.Y);

      private void TriggerHoverEvent<T>(T sender, Cell cell) {
         ColorHover?.Invoke(sender, new ColorSelectionEventArgs {
            Color = GetColorFromCell(cell)
         });
      }

      private void TriggerSelectionEvent<T>(T sender) {
         ColorSelect?.Invoke(sender, new ColorSelectionEventArgs {
            Color = SelectedColor
         });
      }

      private void UpdateSelection(int row, int column, double x, double y) {
         _selectionRow = row;
         _selectionCol = column;

         Canvas.SetLeft(SelectionEllipse, x - 6.5);
         Canvas.SetTop(SelectionEllipse, y - 6.5);

         Canvas.SetLeft(SelectionEllipse2, x - 7.65);
         Canvas.SetTop(SelectionEllipse2, y - 7.65);
      }

      private void UpdateSelection(Cell cell, Point point) =>
         UpdateSelection(cell.Row, cell.Column, point.X, point.Y);

      #region Event Handlers
      private void _pickerCanvas_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(sender as IInputElement);

         position = GetBoundedMouseCoords(position);
         var cell = MouseCoordsToCell(position);

         TriggerHoverEvent(this, cell);

         if (_mousePressed) {
            UpdateSelection(cell, position);
            TriggerSelectionEvent(this);
         }
      }

      private void _pickerCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         var element = sender as IInputElement;

         element?.CaptureMouse();

         _mousePressed = true;

         var position = GetBoundedMouseCoords(e.GetPosition(element));
         var cell = MouseCoordsToCell(position);

         UpdateSelection(cell, position);
         TriggerSelectionEvent(this);
      }

      private void _pickerCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
         _mousePressed = false;

         (sender as IInputElement)?.ReleaseMouseCapture();
      }
      #endregion

   }
}
