// https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/Types/ColorConverters.shared.cs

namespace System.Drawing;

public static class ColorConverters
{
    public static Color FromHex(string hex)
    {
        // Undefined
        if (hex.Length < 3)
            throw new ArgumentException(nameof(hex));

        var idx = (hex[0] == '#') ? 1 : 0;

        switch (hex.Length - idx)
        {
            case 3: // #rgb => ffrrggbb
                var t1 = ToHexD(hex[idx++]);
                var t2 = ToHexD(hex[idx++]);
                var t3 = ToHexD(hex[idx]);

                return Color.FromArgb((int)t1, (int)t2, (int)t3);

            case 4: // #argb => aarrggbb
                var f1 = ToHexD(hex[idx++]);
                var f2 = ToHexD(hex[idx++]);
                var f3 = ToHexD(hex[idx++]);
                var f4 = ToHexD(hex[idx]);
                return Color.FromArgb((int)f1, (int)f2, (int)f3, (int)f4);

            case 6: // #rrggbb => ffrrggbb
                return Color.FromArgb(
                        (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                        (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                        (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])));

            case 8: // #aarrggbb
                var a1 = ToHex(hex[idx++]) << 4 | ToHex(hex[idx++]);
                return Color.FromArgb(
                        (int)a1,
                        (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                        (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                        (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])));

            default: // everything else will result in unexpected results
                throw new ArgumentException(nameof(hex));
        }
    }

    internal static uint ToHexD(char c)
    {
        var j = ToHex(c);
        return (j << 4) | j;
    }

    internal static uint ToHex(char c)
    {
        var x = (ushort)c;
        if (x >= '0' && x <= '9')
            return (uint)(x - '0');

        x |= 0x20;
        if (x >= 'a' && x <= 'f')
            return (uint)(x - 'a' + 10);
        return 0;
    }
}
