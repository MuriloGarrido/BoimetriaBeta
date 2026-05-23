using BoimetriaBeta.ViewModels;

namespace BoimetriaBeta.Views;

public partial class MainPage : ContentPage
{
    readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
        _ = AnimateHeroAsync();
    }

    async Task AnimateHeroAsync()
    {
        HeroCard.Opacity = 0;
        HeroCard.TranslationY = 16;
        await Task.WhenAll(
            HeroCard.FadeToAsync(1, 350, Easing.CubicOut),
            HeroCard.TranslateToAsync(0, 0, 400, Easing.CubicOut));
    }
}
