// Views/CategoryDetailPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class CategoryDetailPage : ContentPage
{
    public CategoryDetailPage(CategoryDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // No OnAppearing logic needed here typically, ViewModel handles loading via QueryProperty
}