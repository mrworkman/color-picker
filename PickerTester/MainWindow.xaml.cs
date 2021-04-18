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

using System.Diagnostics.CodeAnalysis;
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

      public void _colorPicker_OnColorHover(object sender, ColorSelectionEventArgs e) {
         _hoveredColorLabel.Background = new SolidColorBrush(e.Color);
         _hoveredColorLabel.Content = e.Color.ToString();
      }

      public void _colorPicker_OnColorSelect(object sender, ColorSelectionEventArgs e) {
         _selectedColorLabel.Background = new SolidColorBrush(e.Color);
         _selectedColorLabel.Content = e.Color.ToString();
      }

      public void _huePicker_OnHueHover(object sender, HueSelectionEventArgs e) {
         _hoveredHueLabel.Background = new SolidColorBrush(e.HueColor);
         _hoveredHueLabel.Content = e.Hue;
      }

      public void _huePicker_OnHueSelect(object sender, HueSelectionEventArgs e) {
         _colorPicker.Hue = e.Hue;
         _selectedHueLabel.Background = new SolidColorBrush(e.HueColor);
         _selectedHueLabel.Content = e.Hue;
      }

      [SuppressMessage("ReSharper", "InconsistentNaming")]
      public void _colorSwatches_OnColorSelected(object sender, ColorSelectionEventArgs e) {
         var colorBytes = new[] { e.Color.R, e.Color.G, e.Color.B };

         //ColorConversion.GetHsv(colorBytes, out var H, out _, out _);
         var hue = (int) new ColorModel(e.Color).Hue;

         _colorPicker.SelectedColor = e.Color;
         _huePicker.SelectedHue = hue;

         _selectedHueLabel.Content = hue;
      }

   }
}
