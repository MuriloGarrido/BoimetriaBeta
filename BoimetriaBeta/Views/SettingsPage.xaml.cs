using BoimetriaBeta.ViewModels;

namespace BoimetriaBeta.Views;

public partial class SettingsPage : ContentPage
{
    readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
    }
}
