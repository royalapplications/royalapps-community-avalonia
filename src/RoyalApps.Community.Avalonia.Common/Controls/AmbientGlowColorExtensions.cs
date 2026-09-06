using Avalonia.Media;

namespace RoyalApps.Community.Avalonia.Common.Controls;

internal static class AmbientGlowColorExtensions
{
    extension(Color color)
    {
        public Color ChangeBrightness(float correctionFactor)
        {
            var alpha = (float)color.A;
            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb((byte)alpha, (byte)red, (byte)green, (byte)blue);
        }

    }
}
