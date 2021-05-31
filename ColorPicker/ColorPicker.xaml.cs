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

      private int _selectedRow = 0;
      private int _selectedColumn = 0;

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
            SelectColor(value);

            DrawPickerCanvas();
            TriggerSelectionEvent(this);
         }
      }

      private Point GetBoundedMouseCoords(Point p) => new Point {
         X = Math.Max(0.0, Math.Min(PickerCanvas.ActualWidth - 1.0, p.X)),
         Y = Math.Max(0.0, Math.Min(PickerCanvas.ActualHeight - 1.0, p.Y))
      };

      private Color GetColorFromCoords(int row, int column) => ComputeColor(_hue, row, column);
      private Color GetColorFromSelectionCoords() => GetColorFromCoords(_selectedRow, _selectedColumn);

      private Color ComputeColor(int hue, int row, int column) => ColorModel.ComputeColor(
         hue:        hue,
         saturation: column / (double) GridSize,
         value:      (GridSize - row) / (double) GridSize
      );

      private void DrawPickerCanvas() {
         for (int row = 0; row < GridSize; row++) {
            for (int col = 0; col < GridSize; col++) {

               // Figure out what colour is represented by the given coordinates.
               var color = ComputeColor(_hue, row, col);

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

      private int TranslateMouseX(double x) =>
         (int) (x / (PickerCanvas.ActualWidth - 1) * (GridSize - 1));

      private int TranslateMouseY(double y) =>
         (int) (y / (PickerCanvas.ActualHeight - 1) * (GridSize - 1));

      private void TriggerHoverEvent<T>(T sender, Color color) {
         ColorHover?.Invoke(sender, new ColorSelectionEventArgs {
            Color = color
         });
      }

      private void TriggerSelectionEvent<T>(T sender) {
         ColorSelect?.Invoke(sender, new ColorSelectionEventArgs {
            Color = SelectedColor
         });
      }

      private void SelectColor(Color color) {
         var colorModel = new ColorModel(color);

         Hue = (int) colorModel.Hue;

         UpdateSelection(
            brightness: 255 - (int) (colorModel.Brightness * 255.0),
            saturation: (int) (colorModel.Saturation * 255.0),
            x:          colorModel.Saturation * ActualWidth,
            y:          ActualHeight - colorModel.Brightness * ActualHeight
         );
      }

      private void UpdateSelection(int brightness, int saturation, double x, double y) {
         _selectedRow = brightness;
         _selectedColumn = saturation;

         Canvas.SetLeft(InnerSelectionEllipse, x - 6.5);
         Canvas.SetTop(InnerSelectionEllipse, y - 6.5);

         Canvas.SetLeft(OuterSelectionEllipse, x - 7.65);
         Canvas.SetTop(OuterSelectionEllipse, y - 7.65);
      }

      #region Event Handlers
      private void PickerCanvas_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(sender as IInputElement);

         position = GetBoundedMouseCoords(position);

         var saturation = TranslateMouseX(position.X);
         var brightness = TranslateMouseY(position.Y);

         TriggerHoverEvent(this, GetColorFromCoords(brightness, saturation));

         if (_mousePressed) {
            UpdateSelection(brightness, saturation, position.X, position.Y);
            TriggerSelectionEvent(this);
         }
      }

      private void PickerCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         var element = sender as IInputElement;

         element?.CaptureMouse();

         _mousePressed = true;

         var position = GetBoundedMouseCoords(e.GetPosition(element));
         var saturation = TranslateMouseX(position.X);
         var brightness = TranslateMouseY(position.Y);

         UpdateSelection(brightness, saturation, position.X, position.Y);
         TriggerSelectionEvent(this);
      }

      private void PickerCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
         _mousePressed = false;

         (sender as IInputElement)?.ReleaseMouseCapture();
      }

      private void PickerCanvas_OnLoaded(object sender, RoutedEventArgs e) {
         if (!IsVisible) {
            return;
         }

         SelectColor(SelectedColor);
         DrawPickerCanvas();
      }
      #endregion
   }
}
