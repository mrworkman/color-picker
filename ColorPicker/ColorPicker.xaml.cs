using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MrWorkman.Wpf {
   using static ColorConversion;

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

      public class ColorSelectionEventArgs : EventArgs {
         public Color Color { get; internal set; }
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
      }

      private Point GetBoundedMouseCoords(Point p) {
         double x = p.X, y = p.Y;

         if (x < 0) {
            x = 0;
         }

         if (x >= _pickerCanvas.ActualWidth) {
            x = _pickerCanvas.ActualWidth - 1;
         }

         if (y < 0) {
            y = 0;
         }

         if (y >= _pickerCanvas.ActualHeight) {
            y = _pickerCanvas.ActualHeight - 1;
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

               var bytes = GetRgbBytes(Hue, saturation, value);
               _pixels[o + 0] = bytes[0];
               _pixels[o + 1] = bytes[1];
               _pixels[o + 2] = bytes[2];
            }
         }

         var bitmap = new WriteableBitmap(GridSize, GridSize, 96, 96, PixelFormats.Rgb24, null);
         bitmap.WritePixels(new Int32Rect(0, 0, GridSize, GridSize), _pixels, stride, 0);
         _pickerCanvas.Source = bitmap;
      }

      private Cell MouseCoordsToCell(double x, double y) {
         var cell = new Cell {
            Row = (int) (y / (_pickerCanvas.ActualHeight - 1) * (GridSize - 1)),
            Column = (int) (x / (_pickerCanvas.ActualWidth - 1) * (GridSize - 1))
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

         Canvas.SetLeft(_selectionEllipse, x - 6.5);
         Canvas.SetTop(_selectionEllipse, y - 6.5);

         Canvas.SetLeft(_selectionEllipse2, x - 7.65);
         Canvas.SetTop(_selectionEllipse2, y - 7.65);
      }

      private void UpdateSelection(Cell cell, Point point) => UpdateSelection(cell.Row, cell.Column, point.X, point.Y);

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

      private void _pickerCanvas_OnLoaded(object sender, RoutedEventArgs e) {
         UpdateSelection(255, 0, 0.0, _pickerCanvas.ActualHeight);
      }
      #endregion

   }
}
