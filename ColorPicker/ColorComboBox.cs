using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MrWorkman.Wpf {

   [Localizability(LocalizationCategory.ComboBox)]
   //[TemplatePart(Name = EditableTextBoxTemplateName, Type = typeof(TextBox))]
   [TemplatePart(Name = PopupTemplateName, Type = typeof(Popup))]
   //[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ComboBoxItem))]
   public class ColorComboBox : ComboBox {

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr GetCapture();

      //private const string EditableTextBoxTemplateName = "PART_EditableTextBox";
      private const string PopupTemplateName = "PICKER_Popup";
      private const string SwatchesTemplateName = "ColorSwatches";

      private Popup _dropdownPickerPopup;
      private ColorSwatches _colorSwatches;

      public static readonly Color DefaultColor = Colors.Black;

      static ColorComboBox() {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorComboBox),
            new FrameworkPropertyMetadata(typeof(ColorComboBox)));

         EventManager.RegisterClassHandler(typeof(ColorComboBox),
            Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), true);

         EventManager.RegisterClassHandler(typeof(ColorComboBox),
            Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));

         //EventManager.RegisterClassHandler(typeof(ToggleButton),
         //   Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), true);

      }

      public static readonly DependencyProperty IsPickerDropDownOpenProperty = DependencyProperty.Register(
         nameof(IsPickerDropDownOpen), typeof(bool), typeof(ColorComboBox), new FrameworkPropertyMetadata(
            false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,

            OnIsPickerDropDownOpenChanged,
            CoerceIsPickerDropDownOpen
         )
      );

      public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
         nameof(SelectedColorProperty), typeof(Brush), typeof(ColorComboBox), new FrameworkPropertyMetadata(
            new SolidColorBrush(DefaultColor), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
         )
      );

      [Bindable(true)]
      [Browsable(false)]
      [Category("Appearance")]
      public bool IsPickerDropDownOpen {
         get => (bool) GetValue(IsPickerDropDownOpenProperty);
         set => SetValue(IsPickerDropDownOpenProperty, value);
      }

      [Bindable(true)]
      [Browsable(true)]
      public Brush SelectedColor {
         get => (SolidColorBrush) GetValue(SelectedColorProperty);
         set => SetValue(SelectedColorProperty, value);
      }

      private void Close() {
         if (!IsPickerDropDownOpen) {
            return;
         }

         SetCurrentValue(IsDropDownOpenProperty, false);
         SetCurrentValue(IsPickerDropDownOpenProperty, false);
      }

      private static void OnIsPickerDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
         var comboBox = (ColorComboBox) d;

         var newValue = (bool) e.NewValue;
         var oldValue = !newValue;

         if (UIElementAutomationPeer.FromElement(comboBox) is ComboBoxAutomationPeer boxAutomationPeer) {
            boxAutomationPeer.RaisePropertyChangedEvent(
               ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
               (ExpandCollapseState) (oldValue ? 1 : 0),
               (ExpandCollapseState) (newValue ? 1 : 0)
            );
         }

         if (newValue) {
            if (comboBox.IsDropDownOpen) {
               comboBox.SetCurrentValue(IsDropDownOpenProperty, false);
            } else {
               Mouse.Capture(comboBox, CaptureMode.SubTree);
               comboBox.OnDropDownOpened(EventArgs.Empty);
            }
         } else {
            if (comboBox.IsKeyboardFocusWithin) {
               comboBox.Focus();
            }

            if (Mouse.Captured == comboBox) {
               Mouse.Capture(null);
            }

            if (comboBox._dropdownPickerPopup == null) {
               comboBox.OnDropDownClosed(EventArgs.Empty);
            }
         }
      }

      private static object CoerceIsPickerDropDownOpen(DependencyObject d, object value) {
         if ((bool) value) {
            // TODO
         }

         return value;
      }

      public override void OnApplyTemplate() {
         base.OnApplyTemplate();

         if (_dropdownPickerPopup != null) {
            _dropdownPickerPopup.Closed -= OnPopupClosed;
         }

         _dropdownPickerPopup = GetTemplateChild(PopupTemplateName) as Popup;

         if (_dropdownPickerPopup != null) {
            _dropdownPickerPopup.Closed += OnPopupClosed;
         }

         if (_colorSwatches != null) {
            _colorSwatches.ColorSelected -= OnSwatchColorSelected;
         }

         _colorSwatches = GetTemplateChild(SwatchesTemplateName) as ColorSwatches;

         if (_colorSwatches != null) {
            _colorSwatches.ColorSelected += OnSwatchColorSelected;

            Items.Clear();
            foreach (var listViewItem in _colorSwatches._listView.Items) {
               Items.Add(listViewItem);
            }

            _colorSwatches.MouseUp += (sender, args) => IsDropDownOpen = false;
         }
      }

      private static void OnLostMouseCapture(object sender, MouseEventArgs e) {
         var comboBox = (ColorComboBox) sender;

         if (Mouse.Captured == comboBox) {
            return;
         }

         if (e.OriginalSource == comboBox) {
            if (Mouse.Captured != null && IsDescendant(comboBox, Mouse.Captured as DependencyObject)) {
               return;
            }

            comboBox.Close();
         } else if (IsDescendant(comboBox, e.OriginalSource as DependencyObject)) {
            if (!comboBox.IsPickerDropDownOpen || Mouse.Captured != null || !(GetCapture() == IntPtr.Zero)) {
               return;
            }

            Mouse.Capture(comboBox, CaptureMode.SubTree);
            e.Handled = true;
         } else {
            comboBox.Close();
         }
      }

      /// Based on Microsoft's <see cref="MenuBase.IsDescendant" />
      private static bool IsDescendant(DependencyObject reference, DependencyObject node) {
         bool flag = false;

         while (node != null) {
            if (node == reference) {
               flag = true;
               break;
            }

            if (node.GetType().ToString() == "System.Windows.Controls.Primitives.PopupRoot") {
               if ((node as FrameworkElement)?.Parent is Popup parent) {
                  node = parent.Parent ?? parent.PlacementTarget;
               }
            } else {
               node = FindParent(node);
            }
         }

         return flag;
      }

      /// Based on Microsoft's <see cref="PopupControlService.FindParent" />
      private static DependencyObject FindParent(DependencyObject node) {
         var reference1 = node as Visual ?? (DependencyObject) (node as Visual3D);
         var reference2 = reference1 == null ? node as ContentElement : null;

         if (reference2 != null) {
            node = ContentOperations.GetParent(reference2);

            if (node != null) {
               return node;
            }

            if (reference2 is FrameworkContentElement frameworkContentElement) {
               return frameworkContentElement.Parent;
            }
         } else if (reference1 != null) {
            return VisualTreeHelper.GetParent(reference1);
         }

         return null;
      }

      private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e) {
         var comboBox = (ColorComboBox) sender;

         e.Handled = true;

         if (!comboBox.IsDropDownOpen && (Mouse.Captured != comboBox || e.OriginalSource != comboBox)) {
            return;
         }

         comboBox.Close();
      }

      private void OnPopupClosed(object source, EventArgs e) {
         OnDropDownClosed(EventArgs.Empty);
      }

      private void OnSwatchColorSelected(object sender, ColorSelectionEventArgs e) {
         //Text = e.Color.ToString();

         SelectedItem = "#000000";
      }

   }
}
