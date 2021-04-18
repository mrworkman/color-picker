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
using System.Linq;
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
      private const int Dpi = 96;
      private const int GridSize = 256;
      private const int Stride = GridSize * 3;

      private readonly WriteableBitmap _bitmap;
      private readonly byte[] _pixelData = new byte[GridSize * Stride];

      private int _hue;

      private bool _mousePressed = false;

      private int _selectionRow = 0;
      private int _selectionCol = 0;

      private class Cell {
         public int Row { get; set; }
         public int Column { get; set; }
      }

      public ColorPicker() {
         InitializeComponent();

         PickerCanvas.Source = _bitmap = new WriteableBitmap(
            GridSize, GridSize, Dpi, Dpi, PixelFormats.Rgb24, null
         );
      }

      public EventHandler<ColorSelectionEventArgs> ColorHover { get; set; }
      public EventHandler<ColorSelectionEventArgs> ColorSelect { get; set; }

      public int Hue {
         get => _hue;
         set {
            _hue = value;

            DrawPickerCanvas();
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

      private Color GetColor(int row, int column) => ColorModel.ComputeColor(
         Hue, ComputeSaturation(column), ComputeBrightness(row)
      );

      private Color GetColor(Cell cell) => GetColor(cell.Row, cell.Column);

      private Color GetColorFromSelectionCoords() => GetColor(_selectionRow, _selectionCol);

      private double ComputeBrightness(int row) => (GridSize - row) / (double) GridSize;
      private double ComputeSaturation(int column) => column / (double) GridSize;

      private void DrawPickerCanvas() {
         for (int row = 0; row < GridSize; row++) {
            for (int col = 0; col < GridSize; col++) {

               // Figure out what colour is represented by the given coordinates.
               var color = ColorModel.ComputeColor(
                  Hue, ComputeSaturation(col), ComputeBrightness(row)
               );

               // Update our buffer.
               var o = row * Stride + col * 3;
               _pixelData[o + 0] = color.R;
               _pixelData[o + 1] = color.G;
               _pixelData[o + 2] = color.B;

            }
         }

         // Update the canvas.
         _bitmap.WritePixels(new Int32Rect(0, 0, GridSize, GridSize), _pixelData, Stride, 0);
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
            Color = GetColor(cell)
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

         Canvas.SetLeft(InnerSelectionEllipse, x - 6.5);
         Canvas.SetTop(InnerSelectionEllipse, y - 6.5);

         Canvas.SetLeft(OuterSelectionEllipse, x - 7.65);
         Canvas.SetTop(OuterSelectionEllipse, y - 7.65);
      }

      private void UpdateSelection(Cell cell, Point point) =>
         UpdateSelection(cell.Row, cell.Column, point.X, point.Y);

      #region Event Handlers
      private void PickerCanvas_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(sender as IInputElement);

         position = GetBoundedMouseCoords(position);
         var cell = MouseCoordsToCell(position);

         TriggerHoverEvent(this, cell);

         if (_mousePressed) {
            UpdateSelection(cell, position);
            TriggerSelectionEvent(this);
         }
      }

      private void PickerCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         var element = sender as IInputElement;

         element?.CaptureMouse();

         _mousePressed = true;

         var position = GetBoundedMouseCoords(e.GetPosition(element));
         var cell = MouseCoordsToCell(position);

         UpdateSelection(cell, position);
         TriggerSelectionEvent(this);
      }

      private void PickerCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
         _mousePressed = false;

         (sender as IInputElement)?.ReleaseMouseCapture();
      }

      private void PickerCanvas_OnLoaded(object sender, RoutedEventArgs e) {
         DrawPickerCanvas();
      }
      #endregion
   }
}
