using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrWorkman.Wpf {
   public static class ColorConversion {

      [SuppressMessage("ReSharper", "InconsistentNaming")]
      public static byte[] GetRgbBytes(int H, double S, double V) {
         if (H < 0 || H >= 360) {
            throw new ArgumentOutOfRangeException(nameof(H), "Must be between 0 and 360.");
         }

         if (S < 0.0 || S > 1.0) {
            throw new ArgumentOutOfRangeException(nameof(S), "Must be between 0.0 and 1.0.");
         }

         if (V < 0.0 || V > 1.0) {
            throw new ArgumentOutOfRangeException(nameof(V), "Must be between 0.0 and 1.0.");
         }

         var C = V * S;
         var X = C * (1 - Math.Abs((H / 60.0) % 2 - 1));
         var m = V - C;

         byte R = 0, G = 0, B = 0;
         double Rp = 0, Gp = 0, Bp = 0;

         if (H < 60) {
            Rp = C;
            Gp = X;
            Bp = 0;
         } else if (H < 120) {
            Rp = X;
            Gp = C;
            Bp = 0;
         } else if (H < 180) {
            Rp = 0;
            Gp = C;
            Bp = X;
         } else if (H < 240) {
            Rp = 0;
            Gp = X;
            Bp = C;
         } else if (H < 300) {
            Rp = X;
            Gp = 0;
            Bp = C;
         } else if (H < 360) {
            Rp = C;
            Gp = 0;
            Bp = X;
         }

         R = (byte) ((Rp + m) * 255);
         G = (byte) ((Gp + m) * 255);
         B = (byte) ((Bp + m) * 255);

         return new[] { R, G, B };
      }
   }
}
