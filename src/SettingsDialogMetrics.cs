// Single source of truth for the settings dialog layout numbers (D1), named
// after the fts SettingsWindowMetrics convention. Logical values only (96 DPI
// px): the one runtime derivation path is the dialog's S() per-value scaling;
// VerifySettings check 6 mirrors these values and the same formula (D2/D3).
// Spacing values sit on the 4px ladder (S2); structural sizes are derived from
// alignment targets instead - per-line derivations cite the MeasureV4 probe.
internal static class SettingsDialogMetrics
{
    // Spacing ladder (S2): the four rungs this window uses, 8/12/16/20.
    public const int EdgePx = 16;     // window edge on all sides (Win11 content basics)
    public const int LabelGapPx = 8;  // section label -> its content row
    public const int RowGapPx = 12;   // rows inside a group (v3 was 10; snapped onto the ladder)
    public const int GroupGapPx = 20; // between groups (color -> text -> buttons)
    public const int ButtonGapPx = 8; // inside the right-hand button cluster
    public const int CustomButtonGapPx = 8; // last swatch -> custom button (same rung as the swatch pitch's inner gap)

    // Font ladder (S1): Fluent Body 14px / Caption 12px at 96 DPI. Point units
    // carry the window DPI by themselves, so S() never scales them.
    public const float BodyPointSize = 10.5f;
    public const float CaptionPointSize = 9f;

    // Structural sizes (S2): not forced onto the spacing ladder.
    public const int LabelRowHeightPx = 24;   // body line box 18 (Font.Height, MeasureV4) + breathing; v3's 20 was the 9pt-era value
    public const int ContentWidthPx = 408;    // content column width (v1 on; doubles as the button-row budget)
    public const int HintZoneWidthPx = 220;   // caption hint fits on one line at 203px (MeasureV4); section label gets 408-220-8=180
    public const int SwatchSizePx = 28;       // swatch block (v1 on)
    public const int SwatchPitchPx = 36;      // block 28 + inner gap 8 (ladder rung)
    public const int SwatchRowHeightPx = 34;  // row box = custom button height (body 18 + button chrome)
    public const int CustomButtonWidthPx = 116; // text 90 + chrome 8 + margin (flat preferred 102, MeasureV4)
    public const int CustomButtonLiftPx = 3;  // (34-28)/2: swatch and custom button share a center line
    public const int ButtonRowHeightPx = 32;  // Fluent default control height (zero change)
    public const int TextBoxHeightPx = 96;    // 5 body line boxes (5x18; v3's 80 was the 9pt-era value)
    public const int HintHeightPx = 40;       // two caption line boxes (2x16) + padding
    public const int ResetButtonWidthPx = 104;  // text 65 + chrome 8 (flat preferred 77, MeasureV4)
    public const int CancelButtonWidthPx = 80;  // text 37 + chrome 8 (flat minimum 75)
    public const int TryButtonWidthPx = 116;    // text 79 + chrome 8 (flat preferred 91)
    public const int SaveButtonWidthPx = 80;    // text 37 + chrome 8 (flat minimum 75)
    // Button-row budget: 104+80+116+80 + 3x8 = 404 <= ContentWidthPx (S1 checks out with 4px spare).

    // Update footer (V5B): one caption-tier strip between the hint and the
    // button row; both footer buttons take the caption font so they stay
    // compact next to the status line.
    public const int FooterRowHeightPx = 24;    // same box as LabelRowHeightPx: caption line 16 + button chrome margin
    public const int FooterVersionZonePx = 80;  // "版本 1.0.0" caption = 2 CJK 24 + " 1.0.0" ~37 = ~61 + margin
    public const int FooterButtonWidthPx = 80;  // 4-CJK caption text ~48 + chrome (75 flat minimum) -> the 80 tier, like cancel
    // Footer budget: 80 + 8 + 80 + 8 + status + 8 + 80 = 408 -> status line = 144
    // (one caption line, ellipsized overflow); the apply button's slot is fixed
    // so revealing it never shifts any other control (constant geometry).

    public const int PreviewFloorPx = 60;     // preview cap floor (v1 on)
}
