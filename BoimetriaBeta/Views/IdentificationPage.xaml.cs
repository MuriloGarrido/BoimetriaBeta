using BoimetriaBeta.ViewModels;

namespace BoimetriaBeta.Views;

public partial class IdentificationPage : ContentPage
{
    public IdentificationPage(IdentificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
