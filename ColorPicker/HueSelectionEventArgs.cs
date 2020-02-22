using System;
using System.Windows.Media;

namespace MrWorkman.Wpf {
   public class HueSelectionEventArgs : EventArgs {
      public int Hue { get; internal set; }
      public Color HueColor { get; internal set; }
   }
}
