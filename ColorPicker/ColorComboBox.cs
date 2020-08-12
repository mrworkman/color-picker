using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MrWorkman.Wpf {
   public class ColorComboBox : ComboBox {
      static ColorComboBox() {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorComboBox), new FrameworkPropertyMetadata(typeof(ColorComboBox)));
      }

      public static readonly DependencyProperty IsPickerDropDownOpenProperty = DependencyProperty.Register(
         "IsPickerDropDownOpen", typeof(bool), typeof(ColorComboBox), new FrameworkPropertyMetadata(
            false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
         )
      );

      [Bindable(true)]
      public bool IsPickerDropDownOpen {
         get => (bool) GetValue(IsPickerDropDownOpenProperty);
         set => SetValue(IsPickerDropDownOpenProperty, value);
      }



   }
}
