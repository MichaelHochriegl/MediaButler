namespace Frontend.Components.Layout;

using MudBlazor;

public static class MediaButlerTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#D97706",
            Secondary = "#2563EB",
            Tertiary = "#B45309",

            Background = "#F7F3EC",
            Surface = "#FFFFFF",
            AppbarBackground = "#1F1B16",
            AppbarText = "#F5F3EF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#1F1B16",

            TextPrimary = "#1F1B16",
            TextSecondary = "#625B52",

            Divider = "#DED6CB",
            DividerLight = "#E8DED2",

            Error = "#D32F2F",
            Success = "#2E7D32",
            Warning = "#ED6C02",
            Info = "#0288D1"
        },

        PaletteDark = new PaletteDark
        {
            Primary = "#F59E42",
            Secondary = "#6EA8FE",
            Tertiary = "#FFD166",

            Background = "#101014",
            Surface = "#181820",
            AppbarBackground = "#181820",
            AppbarText = "#F5F3EF",
            DrawerBackground = "#14141A",
            DrawerText = "#F5F3EF",

            TextPrimary = "#F5F3EF",
            TextSecondary = "#B8B5AE",

            Divider = "#30303A",
            DividerLight = "#3A3A45",

            Error = "#EF5350",
            Success = "#66BB6A",
            Warning = "#FFCA28",
            Info = "#64B5F6"
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "280px"
        },

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "Arial", "sans-serif"],
                FontSize = "0.95rem",
                LineHeight = "1.5"
            },

            H5 = new H5Typography
            {
                FontWeight = "700",
                LetterSpacing = "-0.02em"
            },

            H6 = new H6Typography
            {
                FontWeight = "700",
                LetterSpacing = "-0.015em"
            },

            Button = new ButtonTypography
            {
                FontWeight = "600",
                TextTransform = "none"
            }
        }
    };
}