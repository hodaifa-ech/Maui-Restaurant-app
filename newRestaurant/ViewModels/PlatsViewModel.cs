// ViewModels/PlatsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models; // Ensure correct namespace
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels // Ensure correct namespace
{
    public partial class PlatsViewModel : BaseViewModel
    {
       

        [ObservableProperty]
        private ObservableCollection<Plat> _plats = new();

        [ObservableProperty]
        private Plat _selectedPlat;

        // Hardcode User ID for cart operations - Replace with real auth later
        private const int CurrentUserId = 1;

        private readonly IPlatService _platService; // ADD
        private readonly ICartService _cartService; // ADD (for AddToCart)
        private readonly INavigationService _navigationService;

        public PlatsViewModel(/*IDataService dataService,*/ IPlatService platService, ICartService cartService, INavigationService navigationService) // CHANGE
        {
            // _dataService = dataService; // REMOVE
            _platService = platService; // ADD
            _cartService = cartService; // ADD
            _navigationService = navigationService;
            Title = "Dishes";
        }
        [RelayCommand]
        private async Task LoadPlatsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Plats.Clear();
                var platsList = await _platService.GetPlatsAsync();
                foreach (var plat in platsList)
                {
                    Plats.Add(plat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading plats: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load dishes.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddPlatAsync()
        {
            if (IsBusy) return;
            await _navigationService.NavigateToAsync(nameof(PlatDetailPage));
        }

        [RelayCommand]
        private async Task GoToPlatDetailAsync()
        {
            if (IsBusy || SelectedPlat == null) return;

            await _navigationService.NavigateToAsync(nameof(PlatDetailPage),
                new Dictionary<string, object>
                {
                    { "PlatId", SelectedPlat.Id }
                });
        }

        [RelayCommand]
        private async Task AddToCartAsync(Plat plat)
        {
            if (plat == null || IsBusy) return;

            IsBusy = true; // Provide visual feedback maybe
            try
            {
                bool success = await _cartService.AddItemToCartAsync(CurrentUserId, plat.Id, 1);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", $"{plat.Name} added to cart.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", $"Failed to add {plat.Name} to cart.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to cart: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An error occurred while adding item to cart.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}