using UnityEngine;

/// <summary>
/// Shared runtime font choices for UI and in-world clue text.
/// Falls back gracefully when a preferred OS font is unavailable.
/// </summary>
public static class RuntimeTypography
{
    private static Font displayFont;
    private static Font bodyFont;

    public static Font GetDisplayFont()
    {
        if (displayFont == null)
        {
            displayFont = CreateFont(
                30,
                "Palatino Linotype",
                "Book Antiqua",
                "Georgia",
                "Garamond",
                "Times New Roman");
        }

        return displayFont;
    }

    public static Font GetBodyFont()
    {
        if (bodyFont == null)
        {
            bodyFont = CreateFont(
                24,
                "Book Antiqua",
                "Georgia",
                "Palatino Linotype",
                "Garamond",
                "Arial");
        }

        return bodyFont;
    }

    private static Font CreateFont(int size, params string[] fontNames)
    {
        for (int i = 0; i < fontNames.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(fontNames[i]))
            {
                continue;
            }

            Font osFont = Font.CreateDynamicFontFromOSFont(fontNames[i], size);
            if (osFont != null)
            {
                return osFont;
            }
        }

        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont == null)
        {
            builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return builtinFont != null ? builtinFont : Font.CreateDynamicFontFromOSFont("Arial", size);
    }
}
