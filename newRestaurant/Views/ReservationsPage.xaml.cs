// Views/ReservationsPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class ReservationsPage : ContentPage
{
    public ReservationsPage(ReservationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReservationsViewModel vm )
        {
            await vm.LoadReservationsCommand.ExecuteAsync(null);
            vm.SelectedReservation = null;
        }
       
    }
}