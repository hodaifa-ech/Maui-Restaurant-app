// Views/LoginPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    // Prevent going back from the login page using hardware/shell back button
    protected override bool OnBackButtonPressed() => true;
}