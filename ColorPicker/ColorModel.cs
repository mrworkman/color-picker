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
using System.Windows.Media;

namespace MrWorkman.Wpf {

   public struct ColorModel {
      private const double Divisor = 255.0;
      private const double Tolerance = 1e-6;

      // Our internal representation.
      private Color _color;

      public byte Red {
         get => Color.R;
         set => Color = Color.FromRgb(value, Color.G, Color.B);
      }

      public byte Green {
         get => Color.G;
         set => Color = Color.FromRgb(Color.R, value, Color.B);
      }

      public byte Blue {
         get => Color.B;
         set => Color = Color.FromRgb(Color.R, Color.G, value);
      }

      private double RedPrime => Red / Divisor;

      private double GreenPrime => Green / Divisor;

      private double BluePrime => Blue / Divisor;

      private double CMax => Math.Max(RedPrime, Math.Max(GreenPrime, BluePrime));
      private double CMin => Math.Min(RedPrime, Math.Min(GreenPrime, BluePrime));
      private double Delta => CMax - CMin;

      public double Hue {
         get => ComputeHue();
         set => Color = ComputeColor(value, Saturation, Brightness);
      }

      public double Saturation {
         get => ComputeSaturation();
         set => Color = ComputeColor(Hue, value, Brightness);
      }

      public double Brightness {
         get => CMax;
         set => Color = ComputeColor(Hue, Saturation, value);
      }

      public Color Color {
         get => _color;
         set => _color = value;
      }

      public ColorModel(Color color) {
         _color = color;
      }

      //public ColorModel(double hue, double saturation, double value) {
      //   _color = ComputeColor(hue, saturation, value);
      //}

      //public ColorModel(byte red, byte green, byte blue)
      //   : this(Color.FromRgb(red, green, blue)) {}

      //public ColorModel(int red, int green, int blue)
      //   : this(Color.FromRgb((byte) red, (byte) green, (byte) blue)) {}

      private static double BoundHue(double hue)               => Math.Max(0.0, Math.Min(360 - Tolerance, hue));
      private static double BoundSaturation(double saturation) => Math.Max(0.0, Math.Min(1.0, saturation));
      private static double BoundBrightness(double value)      => Math.Max(0.0, Math.Min(1.0, value));

      public static Color ComputeColor(double hue, double saturation, double value) {
         hue        = BoundHue(hue);
         saturation = BoundSaturation(saturation);
         value      = BoundBrightness(value);

         var c = value * saturation;
         var x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
         var m = value - c;

         double redPrime = 0, greenPrime = 0, bluePrime = 0;

         if (hue < 60) {
            redPrime   = c;
            greenPrime = x;
            bluePrime  = 0;
         } else if (hue < 120) {
            redPrime   = x;
            greenPrime = c;
            bluePrime  = 0;
         } else if (hue < 180) {
            redPrime   = 0;
            greenPrime = c;
            bluePrime  = x;
         } else if (hue < 240) {
            redPrime   = 0;
            greenPrime = x;
            bluePrime  = c;
         } else if (hue < 300) {
            redPrime   = x;
            greenPrime = 0;
            bluePrime  = c;
         } else if (hue < 360) {
            redPrime   = c;
            greenPrime = 0;
            bluePrime  = x;
         }

         return Color.FromRgb(
            r: (byte) ((redPrime   + m) * 255),
            g: (byte) ((greenPrime + m) * 255),
            b: (byte) ((bluePrime  + m) * 255)
         );
      }

      private double ComputeHue() {
         var hue = 0.0;

         // i.e. delta != 0.
         if (Math.Abs(Delta) > Tolerance) {

            // i.e. CMax == R'.
            if (Math.Abs(CMax - RedPrime) < Tolerance) {
               hue = 60 * ((GreenPrime - BluePrime) / Delta % 6);

            // i.e. CMax == G'.
            } else if (Math.Abs(CMax - GreenPrime) < Tolerance) {
               hue = 60 * ((BluePrime - RedPrime) / Delta + 2);

            // i.e. CMax == B'.
            } else if (Math.Abs(CMax - BluePrime) < Tolerance) {
               hue = 60 * ((RedPrime - GreenPrime) / Delta + 4);
            }

         }

         if (hue < 0) {
            hue += 360.0;
         }

         return hue;
      }

                                         // i.e. CMax != 0.
      private double ComputeSaturation() => Math.Abs(CMax) > Tolerance ? Delta / CMax : 0;
   }
}
