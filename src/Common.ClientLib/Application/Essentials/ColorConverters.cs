//using System.Drawing;

//// ReSharper disable once CheckNamespace
//namespace System.Application
//{
//    public static class ColorConverters
//    {
//        // https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/Types/ColorConverters.shared.cs
//        // 错误	CS0012	类型“Color”在未引用的程序集中定义。必须添加对程序集“OpenTK, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065”的引用。	ST.Client (net6.0-macos)SteamTools\src\ST.Client\UI\ViewModels\Windows\LoginOrRegisterWindowViewModel.cs	309	活动

//        public static Color FromHsl(float hue, float saturation, float luminosity)
//        {
//            ConvertToRgb(hue / 360f, saturation / 100f, luminosity / 100f, out var r, out var g, out var b);
//            return Color.FromArgb(r, g, b);
//        }

//        public static Color FromHsla(float hue, float saturation, float luminosity, int alpha)
//        {
//            ConvertToRgb(hue / 360f, saturation / 100f, luminosity / 100f, out var r, out var g, out var b);
//            return Color.FromArgb(alpha, r, g, b);
//        }

//        public static Color FromHex(string hex)
//        {
//            // Undefined
//            if (hex.Length < 3)
//                throw new ArgumentException(nameof(hex));

//            var idx = (hex[0] == '#') ? 1 : 0;

//            switch (hex.Length - idx)
//            {
//                case 3: // #rgb => ffrrggbb
//                    var t1 = ToHexD(hex[idx++]);
//                    var t2 = ToHexD(hex[idx++]);
//                    var t3 = ToHexD(hex[idx]);

//                    return Color.FromArgb((int)t1, (int)t2, (int)t3);

//                case 4: // #argb => aarrggbb
//                    var f1 = ToHexD(hex[idx++]);
//                    var f2 = ToHexD(hex[idx++]);
//                    var f3 = ToHexD(hex[idx++]);
//                    var f4 = ToHexD(hex[idx]);
//                    return Color.FromArgb((int)f1, (int)f2, (int)f3, (int)f4);

//                case 6: // #rrggbb => ffrrggbb
//                    return Color.FromArgb(
//                            (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
//                            (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
//                            (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])));

//                case 8: // #aarrggbb
//                    var a1 = ToHex(hex[idx++]) << 4 | ToHex(hex[idx++]);
//                    return Color.FromArgb(
//                            (int)a1,
//                            (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
//                            (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
//                            (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])));

//                default: // everything else will result in unexpected results
//                    throw new ArgumentException(nameof(hex));
//            }
//        }

//        public static Color FromUInt(uint argb)
//        {
//            var a = (byte)(argb >> 24);
//            var r = (byte)(argb >> 16);
//            var g = (byte)(argb >> 8);
//            var b = (byte)(argb >> 0);

//            return Color.FromArgb(a, r, g, b);
//        }

//        internal static void ConvertToRgb(float hue, float saturation, float luminosity, out int r, out int g, out int b)
//        {
//            if (luminosity == 0)
//            {
//                r = g = b = 0;
//                return;
//            }

//            if (saturation == 0)
//            {
//                r = g = b = (int)Math.Round(luminosity * 255, MidpointRounding.AwayFromZero);
//                return;
//            }

//            var temp2 = luminosity <= 0.5f ? luminosity * (1.0f + saturation) : luminosity + saturation - (luminosity * saturation);
//            var temp1 = (2.0f * luminosity) - temp2;

//            var t3 = new[] { hue + (1.0f / 3.0f), hue, hue - (1.0f / 3.0f) };
//            var clr = new float[] { 0, 0, 0 };
//            for (var i = 0; i < 3; i++)
//            {
//                if (t3[i] < 0)
//                    t3[i] += 1.0f;
//                if (t3[i] > 1)
//                    t3[i] -= 1.0f;
//                if (6.0 * t3[i] < 1.0)
//                    clr[i] = temp1 + ((temp2 - temp1) * t3[i] * 6.0f);
//                else if (2.0 * t3[i] < 1.0)
//                    clr[i] = temp2;
//                else if (3.0 * t3[i] < 2.0)
//                    clr[i] = temp1 + ((temp2 - temp1) * ((2.0f / 3.0f) - t3[i]) * 6.0f);
//                else
//                    clr[i] = temp1;
//            }

//            r = (int)Math.Round(clr[0] * 255, MidpointRounding.AwayFromZero);
//            g = (int)Math.Round(clr[1] * 255, MidpointRounding.AwayFromZero);
//            b = (int)Math.Round(clr[2] * 255, MidpointRounding.AwayFromZero);
//        }

//        internal static void ConvertToHsl(float r, float g, float b, out float h, out float s, out float l)
//        {
//            var v = Math.Max(r, g);
//            v = Math.Max(v, b);

//            var m = Math.Min(r, g);
//            m = Math.Min(m, b);

//            l = (m + v) / 2.0f;
//            if (l <= 0.0)
//            {
//                h = s = l = 0;
//                return;
//            }
//            var vm = v - m;
//            s = vm;

//            if (s > 0.0)
//            {
//                s /= l <= 0.5f ? v + m : 2.0f - v - m;
//            }
//            else
//            {
//                h = 0;
//                s = 0;
//                return;
//            }

//            var r2 = (v - r) / vm;
//            var g2 = (v - g) / vm;
//            var b2 = (v - b) / vm;

//            if (r == v)
//            {
//                h = g == m ? 5.0f + b2 : 1.0f - g2;
//            }
//            else if (g == v)
//            {
//                h = b == m ? 1.0f + r2 : 3.0f - b2;
//            }
//            else
//            {
//                h = r == m ? 3.0f + g2 : 5.0f - r2;
//            }
//            h /= 6.0f;
//        }

//        internal static uint ToHexD(char c)
//        {
//            var j = ToHex(c);
//            return (j << 4) | j;
//        }

//        internal static uint ToHex(char c)
//        {
//            var x = (ushort)c;
//            if (x >= '0' && x <= '9')
//                return (uint)(x - '0');

//            x |= 0x20;
//            if (x >= 'a' && x <= 'f')
//                return (uint)(x - 'a' + 10);
//            return 0;
//        }
//    }
//}
