using System;
using System.Windows.Media;

namespace MrWorkman.Wpf {
   public class ColorSelectionEventArgs : EventArgs {
      public Color Color { get; internal set; }
   }
}
