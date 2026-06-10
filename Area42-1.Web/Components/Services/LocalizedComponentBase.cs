using Microsoft.AspNetCore.Components;

namespace Area42_1.Web.Components.Services;

public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected AppUiState UiState { get; set; } = default!;

    protected string T(string dutch, string english) => UiState.Language == "nl" ? dutch : english;

    protected override async Task OnInitializedAsync()
    {
        UiState.OnChange += StateHasChanged;
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UiState.InitializeAsync();
        }
    }

    public void Dispose()
    {
        UiState.OnChange -= StateHasChanged;
    }
}
