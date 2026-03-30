using Blazored.LocalStorage;
using MudBlazor;

namespace Frontend.Services;

public interface IThemeService
{
    Task<MudTheme> GetCurrentThemeAsync();
    Task<string> GetCurrentThemeKeyAsync();
    Task SetThemeAsync(string key);
    IReadOnlyDictionary<string, string> GetAvailableThemes();
}

public sealed class ThemeService : IThemeService
{
    private const string ThemeKey = "tw_theme";

    private readonly ILocalStorageService _localStorage;

    private static readonly Dictionary<string, string> Themes = new()
    {
        ["light"] = "Светлая",
        ["dark"] = "Тёмная (по умолчанию)",
        ["warm"] = "Тёплая",
        ["cold"] = "Холодная",
        ["neon"] = "Салатовая"
    };

    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public IReadOnlyDictionary<string, string> GetAvailableThemes() => Themes;

    public async Task<MudTheme> GetCurrentThemeAsync()
    {
        var key = await GetCurrentThemeKeyAsync();
        return key switch
        {
            "light" => BuildLightTheme(),
            "warm" => BuildWarmTheme(),
            "cold" => BuildColdTheme(),
            "neon" => BuildNeonTheme(),
            _ => BuildDarkTheme()
        };
    }

    public async Task<string> GetCurrentThemeKeyAsync()
    {
        var key = await _localStorage.GetItemAsStringAsync(ThemeKey);
        if (string.IsNullOrWhiteSpace(key))
        {
            return "dark";
        }

        return Themes.ContainsKey(key) ? key : "dark";
    }

    public async Task SetThemeAsync(string key)
    {
        if (!Themes.ContainsKey(key))
        {
            key = "dark";
        }

        await _localStorage.SetItemAsStringAsync(ThemeKey, key);
    }

    private static MudTheme BuildDarkTheme() => new()
    {
        PaletteDark = new PaletteDark
        {
            Background = "#050608",
            Surface = "#101218",
            Primary = "#00ff9d",
            PrimaryContrastText = "#050608",
            Secondary = "#00b3ff",
            AppbarBackground = "#050608",
            DrawerBackground = "#0b0d12",
            DrawerText = "#e5e7eb",
            TextPrimary = "#f9fafb",
            TextSecondary = "#9ca3af",
            Success = "#22c55e",
            Warning = "#f97316",
            Error = "#ef4444",
            Info = "#38bdf8"
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "system-ui", "Inter", "Segoe UI", "sans-serif" }
            }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "0px"
        }
    };

    private static MudTheme BuildLightTheme() => new()
    {
        PaletteLight = new PaletteLight
        {
            Background = "#f3f4f6",
            Surface = "#ffffff",
            Primary = "#00ff9d",
            PrimaryContrastText = "#020617",
            TextPrimary = "#020617",
            TextSecondary = "#4b5563"
        }
    };

    private static MudTheme BuildWarmTheme() => new()
    {
        PaletteDark = new PaletteDark
        {
            Background = "#1c1917",
            Surface = "#292524",
            Primary = "#f59e0b",
            PrimaryContrastText = "#020617",
            TextPrimary = "#fef3c7",
            TextSecondary = "#f97316"
        }
    };

    private static MudTheme BuildColdTheme() => new()
    {
        PaletteDark = new PaletteDark
        {
            Background = "#020617",
            Surface = "#020617",
            Primary = "#38bdf8",
            PrimaryContrastText = "#020617",
            TextPrimary = "#e0f2fe",
            TextSecondary = "#7dd3fc"
        }
    };

    private static MudTheme BuildNeonTheme() => new()
    {
        PaletteDark = new PaletteDark
        {
            Background = "#010712",
            Surface = "#020617",
            Primary = "#00ff9d",
            PrimaryContrastText = "#020617",
            Secondary = "#22d3ee",
            TextPrimary = "#e5fdf5",
            TextSecondary = "#a5f3fc"
        }
    };
}

