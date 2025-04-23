// Views/PlatDetailPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class PlatDetailPage : ContentPage
{
    public PlatDetailPage(PlatDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Call the ViewModel's initialization logic when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlatDetailViewModel vm)
        {
            await vm.InitializeAsync(); // Call the async initialization
        }
    }
}