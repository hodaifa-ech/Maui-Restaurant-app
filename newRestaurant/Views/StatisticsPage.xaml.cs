// Views/StatisticsPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class StatisticsPage : ContentPage
{
    public StatisticsPage(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StatisticsViewModel vm && vm.CanViewStatistics) // Only load if allowed
        {
            // Optionally clear previous data before loading
            // vm.TotalRevenue = 0;
            // vm.PopularDishes.Clear();
            await vm.LoadStatisticsCommand.ExecuteAsync(null);
        }
    }
}