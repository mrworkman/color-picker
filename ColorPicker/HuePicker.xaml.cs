using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MrWorkman.Wpf {
   using static ColorConversion;

   /// <summary>
   /// Interaction logic for HueSlider.xaml
   /// </summary>
   public partial class HuePicker : UserControl {

      private bool _pickerMousePressed;
      private bool _thumbMousePressed;

      private int _selectedHue = 0;
      private int _selectionRow = 255;

      public class HueSelectionEventArgs : EventArgs {
         public int Hue { get; internal set; }
         public Color HueColor { get; internal set; }
      }

      public EventHandler<HueSelectionEventArgs> HueHover { get; set; }
      public EventHandler<HueSelectionEventArgs> HueSelect { get; set; }

      public HuePicker() {
         InitializeComponent();
         InitializeCanvas();
      }

      public int SelectedHue {
         get => _selectedHue;
         set {
            _selectedHue = value;
            _selectionRow = TranslateHueToRow(_selectedHue);

            TriggerSelectionEvent(_pickerCanvas);
         }
      }

      private int TranslateHueToRow(int hue) =>
         (int) (_pickerCanvas.ActualHeight - (hue / 359.0 * _pickerCanvas.ActualHeight));

      private double GetBoundedMouseCoord(double y) {
         if (y < 0) {
            y = 0;
         }

         if (y >= _pickerCanvas.ActualHeight) {
            y = _pickerCanvas.ActualHeight;
         }

         return y;
      }

      private Color GetHueColor(int hue) {
         var c = GetRgbBytes(hue, 1.0, 1.0);
         return Color.FromRgb(c[0], c[1], c[2]);
      }

      private void InitializeCanvas() {
         var stride = 3;
         var pixels = new byte[256 * 3];

         for (int row = 0; row < 256; row++) {
            var o = (row * 3);

            var hue = (int) ((255 - row) / 256.0 * 360);

            var bytes = GetRgbBytes(hue, 1.0, 1.0);
            pixels[o + 0] = bytes[0];
            pixels[o + 1] = bytes[1];
            pixels[o + 2] = bytes[2];
         }

         var bitmap = new WriteableBitmap(1, 256, 96, 96, PixelFormats.Rgb24, null);
         bitmap.WritePixels(new Int32Rect(0, 0, 1, 256), pixels, stride, 0);
         _pickerCanvas.Source = bitmap;
      }

      private double TranslateRowToHue(double y) {
         var hue = (_pickerCanvas.ActualHeight - y) / _pickerCanvas.ActualHeight * 360;

         if (hue < 0) {
            hue = 0;
         }

         if (hue >= 360) {
            hue = 359;
         }

         return hue;
      }

      private void TriggerHoverEvent<T>(T sender, double y) {
         HueHover?.Invoke(sender, new HueSelectionEventArgs {
            Hue = (int) TranslateRowToHue(y),
            HueColor = GetHueColor((int) TranslateRowToHue(y))
         });
      }

      private void TriggerSelectionEvent<T>(T sender) {
         HueSelect?.Invoke(_pickerCanvas, new HueSelectionEventArgs {
            Hue = _selectedHue,
            HueColor = GetHueColor(_selectedHue)
         });
      }

      private void UpdateSelection(double y) {
         _selectionRow = (int) y;
         _selectedHue = (int) TranslateRowToHue(y);
         Canvas.SetTop(_thumb1, y - 3);
      }

      #region Event Handlers
      private void _pickerCanvas_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(sender as IInputElement);

         var y = GetBoundedMouseCoord(position.Y);

         TriggerHoverEvent(sender, y);

         if (_pickerMousePressed) {
            UpdateSelection(y);
            TriggerSelectionEvent(sender);
         }
      }

      private void _pickerCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         var element = sender as IInputElement;

         element?.CaptureMouse();
         _pickerMousePressed = true;

         UpdateSelection(GetBoundedMouseCoord(e.GetPosition(element).Y));
         TriggerSelectionEvent(sender);
      }
      private void _pickerCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
         _pickerMousePressed = false;
         (sender as IInputElement)?.ReleaseMouseCapture();
      }

      private void _pickerCanvas_OnLoaded(object sender, RoutedEventArgs e) {
         _selectionRow = TranslateHueToRow(_selectedHue);
         UpdateSelection(_selectionRow);

         // Fire selection event
         SelectedHue = _selectedHue;
      }

      private void _thumb1_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(_pickerCanvas as IInputElement);

         var y = GetBoundedMouseCoord(position.Y);

         if (_thumbMousePressed) {
            UpdateSelection(y);
            TriggerSelectionEvent(sender);
         }
      }

      private void _thumb1_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         var element = sender as IInputElement;
         element?.CaptureMouse();
         _thumbMousePressed = true;
      }

      private void _thumb1_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
         _thumbMousePressed = false;
         (sender as IInputElement)?.ReleaseMouseCapture();
      }
      #endregion
   }
}
