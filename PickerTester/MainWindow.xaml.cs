using System.Windows;
using System.Windows.Media;

using MrWorkman.Wpf;

namespace PickerTester {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window {
      public MainWindow() {
         InitializeComponent();

         _selectedColorLabel.Background = new SolidColorBrush(_colorPicker.SelectedColor);
         _selectedColorLabel.Content = _colorPicker.SelectedColor.ToString();
      }

      public void _colorPicker_OnColorHover(object sender, ColorPicker.ColorSelectionEventArgs e) {
         _hoveredColorLabel.Background = new SolidColorBrush(e.Color);
         _hoveredColorLabel.Content = e.Color.ToString();
      }

      public void _colorPicker_OnColorSelect(object sender, ColorPicker.ColorSelectionEventArgs e) {
         _selectedColorLabel.Background = new SolidColorBrush(e.Color);
         _selectedColorLabel.Content = e.Color.ToString();
      }

      public void _huePicker_OnHueHover(object sender, HuePicker.HueSelectionEventArgs e) {
         _hoveredHueLabel.Background = new SolidColorBrush(e.HueColor);
         _hoveredHueLabel.Content = e.Hue;
      }

      public void _huePicker_OnHueSelect(object sender, HuePicker.HueSelectionEventArgs e) {
         _colorPicker.Hue = e.Hue;
         _selectedHueLabel.Background = new SolidColorBrush(e.HueColor);
         _selectedHueLabel.Content = e.Hue;
      }

   }
}
