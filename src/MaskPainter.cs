using System;
using System.Drawing;
using System.Drawing.Text;

// Shared mask painter: the overlay windows and the settings preview render through
// this single method, so the preview is the same relative layout as the real mask.
internal static class MaskPainter
{
    private static readonly Color LightTextColor = Color.FromArgb(0xF5, 0xF5, 0xF5);
    private static readonly Color DarkTextColor = Color.FromArgb(0x11, 0x11, 0x11);
    private const float MinFontPixels = 12f;

    public static Color PickTextColor(Color background)
    {
        double backgroundLuminance = RelativeLuminance(background);
        double light = ContrastRatio(backgroundLuminance, RelativeLuminance(LightTextColor));
        double dark = ContrastRatio(backgroundLuminance, RelativeLuminance(DarkTextColor));
        return light >= dark ? LightTextColor : DarkTextColor;
    }

    public static void DrawText(Graphics graphics, Rectangle area, MaskConfig config)
    {
        if (area.Width <= 0 || area.Height <= 0 || string.IsNullOrEmpty(config.MaskText)) return;
        using (StringFormat format = new StringFormat())
        {
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.Trimming = StringTrimming.None;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            // The bounds of a PerMonitorV2 window are physical pixels, so a font of
            // area height / 10 pixel units already carries the per-monitor DPI scale.
            float size = area.Height / 10f;
            while (true)
            {
                using (Font font = new Font(FontFamily.GenericSansSerif, size, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    SizeF measured = graphics.MeasureString(config.MaskText, font, area.Size, format);
                    if ((measured.Width <= area.Width && measured.Height <= area.Height) || size <= MinFontPixels)
                    {
                        using (SolidBrush brush = new SolidBrush(PickTextColor(config.MaskColor)))
                            graphics.DrawString(config.MaskText, font, brush,
                                new RectangleF(area.X, area.Y, area.Width, area.Height), format);
                        return;
                    }
                }
                size *= 0.9f;
            }
        }
    }

    private static double RelativeLuminance(Color color)
    {
        return 0.2126 * Linearize(color.R / 255.0)
             + 0.7152 * Linearize(color.G / 255.0)
             + 0.0722 * Linearize(color.B / 255.0);
    }

    private static double Linearize(double channel)
    {
        return channel <= 0.03928 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static double ContrastRatio(double first, double second)
    {
        double lighter = Math.Max(first, second);
        double darker = Math.Min(first, second);
        return (lighter + 0.05) / (darker + 0.05);
    }
}
