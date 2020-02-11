using System;
using System.Diagnostics;
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
      public const int Rows = 256;
      private const int Degrees = 360;

      private bool _pickerMousePressed;
      private bool _thumbMousePressed;

      private int _selectedHue = 0;
      private int _selectionRow = 255;

      public HuePicker() {
         InitializeComponent();
         InitializeCanvas();
      }

      public class HueSelectionEventArgs : EventArgs {
         public int Hue { get; internal set; }
         public Color HueColor { get; internal set; }
      }

      public EventHandler<HueSelectionEventArgs> HueHover { get; set; }
      public EventHandler<HueSelectionEventArgs> HueSelect { get; set; }

      public int SelectedHue {
         get => _selectedHue;
         set {
            _selectedHue = value;

            if (_selectedHue > Degrees - 1 || _selectedHue < 0) {
               throw new ArgumentOutOfRangeException(
                  nameof(SelectedHue), _selectedHue, $"Hue must be between 0 and {Degrees - 1}."
               );
            }

            _selectionRow = GetRowFromHueValue(_selectedHue);

            UpdateSelection(_selectionRow, RowToMouseCoord(_selectionRow));

            TriggerSelectionEvent(this);
         }
      }

      private double GetBoundedMouseCoord(double y) {
         if (y < 0) {
            y = 0;
         }

         if (y >= _pickerCanvas.ActualHeight) {
            y = _pickerCanvas.ActualHeight - 1;
         }

         return y;
      }

      private int GetHueValueFromRow(int row) => (int) ((double) row / (Rows - 1) * (Degrees - 1));

      private Color GetHueColorFromValue(int hue) {
         var c = GetRgbBytes(hue, 1.0, 1.0);
         return Color.FromRgb(c[0], c[1], c[2]);
      }

      private int GetRowFromHueValue(int hue) => (int) ((double) hue / (Degrees - 1) * (Rows - 1));

      private void InitializeCanvas() {
         var stride = 3;
         var pixels = new byte[Rows * 3];

         for (int row = 0; row < Rows; row++) {
            var o = (row * 3);

            var hue = (int) ((double) (Rows - 1 - row) / Rows * Degrees);

            var bytes = GetRgbBytes(hue, 1.0, 1.0);
            pixels[o + 0] = bytes[0];
            pixels[o + 1] = bytes[1];
            pixels[o + 2] = bytes[2];
         }

         var bitmap = new WriteableBitmap(1, Rows, 96, 96, PixelFormats.Rgb24, null);
         bitmap.WritePixels(new Int32Rect(0, 0, 1, Rows), pixels, stride, 0);
         _pickerCanvas.Source = bitmap;
      }

      private int MouseCoordToRow(double y) {
         var row = (int) (_pickerCanvas.ActualHeight - 1 - y / (_pickerCanvas.ActualHeight - 1) * (Rows - 1));

         Debug.WriteLine($"row: {row}");

         return row;
      }

      private double RowToMouseCoord(int row) {
         var y = _pickerCanvas.ActualHeight - 1 - (double) row / (Rows - 1) * (_pickerCanvas.ActualHeight - 1);

         Debug.WriteLine($"y: {y}");

         return y;
      }

      private void TriggerHoverEvent<T>(T sender, int row) {
         HueHover?.Invoke(sender, new HueSelectionEventArgs {
            Hue = GetHueValueFromRow(row),
            HueColor = GetHueColorFromValue(GetHueValueFromRow(row))
         });
      }

      private void TriggerSelectionEvent<T>(T sender) {
         HueSelect?.Invoke(sender, new HueSelectionEventArgs {
            Hue = _selectedHue,
            HueColor = GetHueColorFromValue(_selectedHue)
         });
      }

      private void UpdateSelection(int row, double y) {
         _selectionRow = row;
         _selectedHue = GetHueValueFromRow(row);

         Canvas.SetTop(_thumb1, y - 4.3);
      }

      #region Event Handlers
      private void _pickerCanvas_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(sender as IInputElement);

         var y = GetBoundedMouseCoord(position.Y);
         var row = MouseCoordToRow(y);

         TriggerHoverEvent(sender, row);

         if (_pickerMousePressed) {
            UpdateSelection(row, y);
            TriggerSelectionEvent(this);
         }
      }

      private void _pickerCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         var element = sender as IInputElement;

         element?.CaptureMouse();
         _pickerMousePressed = true;

         var y = GetBoundedMouseCoord(e.GetPosition(element).Y);
         var row = MouseCoordToRow(y);

         UpdateSelection(row, y);
         TriggerSelectionEvent(this);
      }

      private void _pickerCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
         _pickerMousePressed = false;
         (sender as IInputElement)?.ReleaseMouseCapture();
      }

      private void _pickerCanvas_OnLoaded(object sender, RoutedEventArgs e) {
         _selectionRow = GetRowFromHueValue(_selectedHue);
         UpdateSelection(_selectionRow, RowToMouseCoord(_selectionRow));

         // Fire selection event
         SelectedHue = _selectedHue;
      }

      private void _thumb1_OnMouseMove(object sender, MouseEventArgs e) {
         var position = e.GetPosition(_pickerCanvas as IInputElement);

         var y = GetBoundedMouseCoord(position.Y);
         var row = MouseCoordToRow(y);

         if (_thumbMousePressed) {
            UpdateSelection(row, y);
            TriggerSelectionEvent(this);
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
