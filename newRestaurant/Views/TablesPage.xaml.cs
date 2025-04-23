// Views/TablesPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

// Ensure class name matches file name and XAML x:Class
public partial class TablesPage : ContentPage
{
    public TablesPage(TablesViewModel viewModel) // Inject correct ViewModel
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Always reload data when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TablesViewModel vm)
        {
            await vm.LoadTablesCommand.ExecuteAsync(null); // Execute the load command
            vm.SelectedTable = null; // Clear selection after loading/reloading
        }
    }
}