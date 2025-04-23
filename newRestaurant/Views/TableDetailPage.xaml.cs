// Views/TableDetailPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class TableDetailPage : ContentPage
{
    // Inject the specific ViewModel
    public TableDetailPage(TableDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Set BindingContext
    }

    // Call the ViewModel's initialization logic when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TableDetailViewModel vm)
        {
            await vm.InitializeAsync(); // Call the async initialization
        }
    }
}