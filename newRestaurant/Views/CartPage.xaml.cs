// Views/CartPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class CartPage : ContentPage
{
    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Always reload cart when the page appears to ensure it's up-to-date
        if (BindingContext is CartViewModel vm)
        {
            await vm.LoadCartCommand.ExecuteAsync(null);
        }
    }
}