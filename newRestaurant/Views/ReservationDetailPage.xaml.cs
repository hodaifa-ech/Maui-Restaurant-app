// Views/ReservationDetailPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class ReservationDetailPage : ContentPage
{
    public ReservationDetailPage(ReservationDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Call the ViewModel's initialization logic when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReservationDetailViewModel vm)
        {
            await vm.InitializeAsync(); // Call the async initialization
        }
    }
}