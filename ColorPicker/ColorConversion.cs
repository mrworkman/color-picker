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
using System.Diagnostics.CodeAnalysis;

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

      [SuppressMessage("ReSharper", "InconsistentNaming")]
      [SuppressMessage("ReSharper", "CommentTypo")]
      public static void GetHsv(byte[] bytes, out int H, out double S, out double V) {
         if (bytes == null || bytes.Length != 3) {
            throw new ArgumentException("Byte array needs to be 3 bytes long.", nameof(bytes));
         }

         byte R = 0, G = 0, B = 0;
         double Rp = 0, Gp = 0, Bp = 0;

         R = bytes[0];
         G = bytes[1];
         B = bytes[2];

         Rp = R / 255.0;
         Gp = G / 255.0;
         Bp = B / 255.0;

         double Cmax = Math.Max(Rp, Math.Max(Gp, Bp));
         double Cmin = Math.Min(Rp, Math.Min(Gp, Bp));
         double delta = Cmax - Cmin;

         const double tolerance = 1e-6;

         // Value.
         V = Cmax;

         // Saturation.
         if (Math.Abs(Cmax) > tolerance) { // i.e. Cmax != 0
            S = delta / Cmax;
         } else {
            S = 0;
         }

         H = 0;

         // Hue.
         if (Math.Abs(delta) > tolerance) { // i.e. delta != 0
            if (Math.Abs(Cmax - Rp) < tolerance) { // i.e. Cmax == R'
               H = (int) (60 * ((Gp - Bp) / delta % 6));
            } else if (Math.Abs(Cmax - Gp) < tolerance) { // i.e. Cmax == G'
               H = (int) (60 * (((Bp - Rp) / delta) + 2));
            } else if (Math.Abs(Cmax - Bp) < tolerance) { // i.e. Cmax == B'
               H = (int) (60 * (((Rp - Gp) / delta) + 4));
            }
         }

         if (H < 0) {
            H += 360;
         }
      }
   }
}
