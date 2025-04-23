// Views/CategoriesPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class CategoriesPage : ContentPage
{
    // Inject ViewModel via constructor
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Optional: Override OnAppearing to load data
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CategoriesViewModel vm) // Load only once or when needed
        {
            await vm.LoadCategoriesCommand.ExecuteAsync(null);
            vm.SelectedCategory = null;

        }
       
    }
}