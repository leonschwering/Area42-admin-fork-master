using Microsoft.JSInterop;

namespace Area42_1.Web.Components.Services;

public sealed class AppUiState
{
    private readonly IJSRuntime _jsRuntime;
    private bool _initialized;

    public AppUiState(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public string Language { get; private set; } = "nl";
    public string Theme { get; private set; } = "light";

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        var language = await _jsRuntime.InvokeAsync<string>("area42Ui.getLanguage");
        var theme = await _jsRuntime.InvokeAsync<string>("area42Ui.getTheme");

        if (!string.IsNullOrWhiteSpace(language))
        {
            Language = language;
        }

        if (!string.IsNullOrWhiteSpace(theme))
        {
            Theme = theme;
        }

        await _jsRuntime.InvokeVoidAsync("area42Ui.applyTheme", Theme);
        OnChange?.Invoke();
    }

    public async Task ToggleLanguageAsync()
    {
        await SetLanguageAsync(Language == "nl" ? "en" : "nl");
    }

    public async Task ToggleThemeAsync()
    {
        await SetThemeAsync(Theme == "dark" ? "light" : "dark");
    }

    public async Task SetLanguageAsync(string language)
    {
        Language = language;
        await _jsRuntime.InvokeVoidAsync("area42Ui.setLanguage", language);
        OnChange?.Invoke();
    }

    public async Task SetThemeAsync(string theme)
    {
        Theme = theme;
        await _jsRuntime.InvokeVoidAsync("area42Ui.applyTheme", theme);
        await _jsRuntime.InvokeVoidAsync("area42Ui.setTheme", theme);
        OnChange?.Invoke();
    }
}
