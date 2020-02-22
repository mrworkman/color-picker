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
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace MrWorkman.Wpf {
   /// <summary>
   /// Interaction logic for ColorSwatch.xaml
   /// </summary>
   public partial class ColorSwatches : UserControl {

      public EventHandler<ColorSelectionEventArgs> ColorSelected { get; set; }

      public ColorSwatches() {
         InitializeComponent();

         var colorValues = new List<string> {
            "#ffffff", "#adadad", "#828282", "#575757", "#2b2b2b", "#000000",
            "#ffffbe", "#ffff99", "#ffff4d", "#ffff00", "#b3b300", "#666600",
            "#ffe7be", "#ffdb99", "#ffc14d", "#ffa500", "#b37400", "#664200",
            "#f6dbc6", "#f3c6a5", "#ea9a62", "#e06f1f", "#9d4e15", "#70380f",
            "#ffbebe", "#ff9999", "#ff4d4d", "#ff0000", "#b30000", "#660000",
            "#ffd9de", "#ffb3bf", "#ff6680", "#ff1a40", "#cc0022", "#800015",
            "#ffc2ff", "#ff99ff", "#ff4dff", "#ff00ff", "#b300b3", "#660066",
            "#e1c9f8", "#cda5f3", "#a862ea", "#8a2be2", "#6918b4", "#410f70",
            "#c2c2ff", "#9999ff", "#4d4dff", "#0000ff", "#0000b3", "#000066",
            "#bfffff", "#99ffff", "#4cffff", "#00ffff", "#00b3b3", "#006666",
            "#c2ffc2", "#99ff99", "#4dff4d", "#00ff00", "#00b300", "#006600",
         };

         _listView.ItemsSource = colorValues;
      }

      private void _listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
         var color = (Color) (ColorConverter.ConvertFromString((string) e.AddedItems[0]) ?? Colors.Black);

         ColorSelected?.Invoke(sender, new ColorSelectionEventArgs {
            Color = color
         });
      }
   }
}
