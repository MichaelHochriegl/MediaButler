using MudBlazor;

namespace Frontend.Components.Layout;

public static class MediaButlerTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Black = "#17181C",
            White = "#FFFFFF",

            // Brand
            Primary = "#FF8A4C",
            PrimaryContrastText = "#FFFFFF",

            // Calm steel tone for secondary actions
            Secondary = "#52657A",
            SecondaryContrastText = "#FFFFFF",

            // Sparingly used supporting accent
            Tertiary = "#0F766E",
            TertiaryContrastText = "#FFFFFF",

            // Semantic colors
            Info = "#2563EB",
            InfoContrastText = "#FFFFFF",

            Success = "#15803D",
            SuccessContrastText = "#FFFFFF",

            Warning = "#D97706",
            WarningContrastText = "#1C1917",

            Error = "#DC2626",
            ErrorContrastText = "#FFFFFF",

            // Page and component surfaces
            Background = "#F6F7F9",
            BackgroundGray = "#EEF0F3",
            Surface = "#FFFFFF",

            // Modern light shell
            AppbarBackground = "#FFFFFF",
            AppbarText = "#17181C",

            DrawerBackground = "#F9FAFB",
            DrawerText = "#30333A",
            DrawerIcon = "#737985",

            // Content
            TextPrimary = "#17181C",
            TextSecondary = "#656B76",
            TextDisabled = "#A3A7AF",

            ActionDefault = "#656B76",
            ActionDisabled = "#B7BBC2",
            ActionDisabledBackground = "#ECEEF1",

            // Dividers and inputs
            LinesDefault = "#E2E5EA",
            LinesInputs = "#D5DAE1",
            Divider = "#E2E5EA",
            DividerLight = "#EEF0F3",

            // Tables and lists
            TableLines = "#E7E9ED",
            TableStriped = "rgba(23, 24, 28, 0.018)",
            TableHover = "rgba(23, 24, 28, 0.045)",

            Skeleton = "rgba(23, 24, 28, 0.09)",

            HoverOpacity = 0.055,
            RippleOpacity = 0.08
        },

        PaletteDark = new PaletteDark
        {
            Black = "#08090B",
            White = "#FFFFFF",

            // Brighter brand color against dark surfaces
            Primary = "#FF8A4C",
            PrimaryContrastText = "#211009",

            Secondary = "#91A4B7",
            SecondaryContrastText = "#101317",

            Tertiary = "#5EEAD4",
            TertiaryContrastText = "#09201E",

            Info = "#60A5FA",
            InfoContrastText = "#081526",

            Success = "#4ADE80",
            SuccessContrastText = "#071A0D",

            Warning = "#FBBF24",
            WarningContrastText = "#211704",

            Error = "#F87171",
            ErrorContrastText = "#250808",

            Background = "#0D0F12",
            BackgroundGray = "#12151A",
            Surface = "#171A20",

            AppbarBackground = "#12151A",
            AppbarText = "#F4F4F5",

            DrawerBackground = "#101318",
            DrawerText = "#D8DADE",
            DrawerIcon = "#9298A3",

            TextPrimary = "#F4F4F5",
            TextSecondary = "#A8ADB7",
            TextDisabled = "#666B75",

            ActionDefault = "#A8ADB7",
            ActionDisabled = "#5D626C",
            ActionDisabledBackground = "#20232A",

            LinesDefault = "#2A2E36",
            LinesInputs = "#343942",
            Divider = "#2A2E36",
            DividerLight = "#21242B",

            TableLines = "#292D35",
            TableStriped = "rgba(255, 255, 255, 0.018)",
            TableHover = "rgba(255, 255, 255, 0.055)",

            Skeleton = "rgba(255, 255, 255, 0.08)",

            HoverOpacity = 0.07,
            RippleOpacity = 0.09
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "264px"
        },

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily =
                [
                    "Inter",
                    "Roboto",
                    "-apple-system",
                    "BlinkMacSystemFont",
                    "Segoe UI",
                    "sans-serif"
                ],
                FontSize = "0.9375rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "-0.005em"
            },

            H4 = new H4Typography
            {
                FontWeight = "700",
                LetterSpacing = "-0.035em",
                LineHeight = "1.15"
            },

            H5 = new H5Typography
            {
                FontWeight = "700",
                LetterSpacing = "-0.025em",
                LineHeight = "1.2"
            },

            H6 = new H6Typography
            {
                FontWeight = "650",
                LetterSpacing = "-0.015em",
                LineHeight = "1.25"
            },

            Button = new ButtonTypography
            {
                FontWeight = "600",
                LetterSpacing = "0",
                TextTransform = "none"
            }
        }
    };
}
