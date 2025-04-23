// Views/PlatsPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class PlatsPage : ContentPage
{
    public PlatsPage(PlatsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlatsViewModel vm)
        {
            await vm.LoadPlatsCommand.ExecuteAsync(null);
            vm.SelectedPlat = null;

        }
       
    }
}