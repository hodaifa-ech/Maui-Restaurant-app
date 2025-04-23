// Views/RegisterPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}