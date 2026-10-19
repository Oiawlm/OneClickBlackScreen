using System;
using System.Drawing;
using Microsoft.Win32;

// Immutable light/dark palettes for the settings window chrome plus the system
// app-theme probe. Only the settings window uses this: the mask, the preview and
// the swatch colors always come from the configuration (V1/V4/V5).
internal struct ThemePalette
{
    public readonly Color Bg;
    public readonly Color Surface;
    public readonly Color Text;
    public readonly Color TextSecondary;
    public readonly Color Accent;
    public readonly Color AccentText;
    public readonly Color AccentHover;
    public readonly Color AccentPressed;
    public readonly Color Border;
    public readonly Color ButtonHover;
    public readonly Color ButtonPressed;

    private ThemePalette(Color bg, Color surface, Color text, Color textSecondary,
        Color accent, Color accentText, Color accentHover, Color accentPressed,
        Color border, Color buttonHover, Color buttonPressed)
    {
        Bg = bg;
        Surface = surface;
        Text = text;
        TextSecondary = textSecondary;
        Accent = accent;
        AccentText = accentText;
        AccentHover = accentHover;
        AccentPressed = accentPressed;
        Border = border;
        ButtonHover = buttonHover;
        ButtonPressed = buttonPressed;
    }

    // Accent label contrast (WCAG 4.5:1) against every primary-button state:
    // light white-on-accent 5.67 / 4.70 / 6.97, dark black-on-accent 10.47 / 11.52 / 8.41.
    public static readonly ThemePalette Light = new ThemePalette(
        Color.FromArgb(0xF5, 0xF5, 0xF5),
        Color.FromArgb(0xFF, 0xFF, 0xFF),
        Color.FromArgb(0x1A, 0x1A, 0x1A),
        Color.FromArgb(0x61, 0x61, 0x61),
        Color.FromArgb(0x00, 0x67, 0xC0),
        Color.FromArgb(0xFF, 0xFF, 0xFF),
        Color.FromArgb(0x1A, 0x76, 0xC8),
        Color.FromArgb(0x00, 0x5B, 0xA1),
        Color.FromArgb(0xD9, 0xD9, 0xD9),
        Color.FromArgb(0xEA, 0xEA, 0xEA),
        Color.FromArgb(0xD5, 0xE5, 0xF7));

    public static readonly ThemePalette Dark = new ThemePalette(
        Color.FromArgb(0x20, 0x20, 0x20),
        Color.FromArgb(0x2B, 0x2B, 0x2B),
        Color.FromArgb(0xF2, 0xF2, 0xF2),
        Color.FromArgb(0x9E, 0x9E, 0x9E),
        Color.FromArgb(0x4C, 0xC2, 0xFF),
        Color.FromArgb(0x00, 0x00, 0x00),
        Color.FromArgb(0x63, 0xCB, 0xFF),
        Color.FromArgb(0x2F, 0xB0, 0xE0),
        Color.FromArgb(0x45, 0x45, 0x45),
        Color.FromArgb(0x37, 0x37, 0x37),
        Color.FromArgb(0x1F, 0x3A, 0x4D));

    // Registry.GetValue needs the spelled-out root name; a missing or invalid
    // value falls back to light (V1).
    public static bool AppsUseLightTheme()
    {
        try
        {
            object value = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", null);
            if (value is int) return (int)value != 0;
        }
        catch { }
        return true;
    }
}
