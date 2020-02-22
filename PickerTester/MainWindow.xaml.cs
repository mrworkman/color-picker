using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
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

         ColorConversion.GetHsv(colorBytes, out var H, out _, out _);

         _colorPicker.SelectedColor = e.Color;
         _huePicker.SelectedHue = H;

         _selectedHueLabel.Content = H;
      }

   }
}
