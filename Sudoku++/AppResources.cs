using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sudoku
{
    static class AppResources
    {
        public const bool DevMode = true;

        private static Brush GetFrozenBrush(byte a, byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            brush.Freeze();
            return brush;
        }

        public static FontFamily FontFamily { get; } = new FontFamily("Droid Sans");

        public static Typeface BoldTypeface { get; } = new Typeface(FontFamily,
            FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        public static Typeface RegularTypeface { get; } = new Typeface(FontFamily,
            FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        public static Brush BackgroundBrush { get; } = GetFrozenBrush(0xff, 0xf0, 0xf0, 0xf0);
        public static Brush TextBrush { get; } = GetFrozenBrush(0xff, 0x20, 0x20, 0x20);
        public static Brush ValueBrush { get; } = GetFrozenBrush(0xff, 0x10, 0x80, 0xd0);
        public static Brush HighlightBrush { get; } = GetFrozenBrush(0x40, 0x20, 0x20, 0x20);
        public static Brush ArrowBrush { get; } = GetFrozenBrush(0xff, 0xf0, 0x20, 0x20);
        public static Brush RegionHighlightBrush = GetFrozenBrush(0x80, 0xa0, 0xe0, 0xa0);

        public static Brush[] HintNodeBrushes = new Brush[]
        {
            GetFrozenBrush(0x80, 0xff, 0x20, 0x20), // 0 = red
            GetFrozenBrush(0x80, 0x20, 0xa0, 0x20), // 1 = green
            GetFrozenBrush(0x80, 0x20, 0x70, 0xff), // 2 = blue
            GetFrozenBrush(0x80, 0xff, 0xa0, 0x20), // 3 = orange
            GetFrozenBrush(0x80, 0x80, 0x50, 0xf0), // 4 = purple
            GetFrozenBrush(0x80, 0xa0, 0x60, 0x40), // 5 = brown
            null,
            null,
            null,
            null,
            null,
            GetFrozenBrush(0x40, 0x20, 0xa0, 0x20), // 11 = light green
            GetFrozenBrush(0x40, 0x20, 0x70, 0xff), // 12 = light blue
            GetFrozenBrush(0x40, 0xff, 0xa0, 0x20), // 13 = light orange
            GetFrozenBrush(0x40, 0x80, 0x50, 0xf0), // 14 = light purple
            GetFrozenBrush(0x40, 0xa0, 0x60, 0x40), // 15 = light brown
        };

        public static Brush GetRainbowBrush(int total, int index)
        {
	        // 0 = red, 1 = green, 2 = blue, 3 = red
	        double hue = 3.0 * index / total;

                double red = 0, green = 0, blue = 0;
	        if (hue <= .5 || hue >= 2.5) red = 1;
	        else if (hue< 1.5) red = (1.5 - hue);
	        else if (hue > 2) red = (hue - 2) * 2;

	        if (hue >= .5 && hue <= 1.5) green = 1;
	        else if (hue< .5) green = hue* 2;
	        else if (hue< 2) green = (2 - hue) * 2;

	        if (hue >= 1.5 && hue <= 2.5) blue = 1;
	        else if (hue > 2.5) blue = (3 - hue) * 2;
	        else if (hue > 1) blue = (hue - 1) * 2;

	        byte a = 0xa0,
                r = (byte)(red* 0x4f + 0xa0),
		        g = (byte)(green* 0x3f + 0xc0),
		        b = (byte)(blue* 0x4f + 0xa0);

	        return GetFrozenBrush(a, r, g, b);
        }
    }
}
