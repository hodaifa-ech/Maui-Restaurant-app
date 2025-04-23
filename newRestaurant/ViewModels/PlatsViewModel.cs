// ViewModels/PlatsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.Views;
using System.Collections.ObjectModel;
using System.ComponentModel; // Required for PropertyChangedEventArgs
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class PlatsViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Plat> _plats = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToPlatDetailCommand))] // Update CanExecute when selection changes
        private Plat _selectedPlat;

        // --- Role-based Property ---
       

        private readonly IPlatService _platService;
        private readonly ICartService _cartService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService; // *** ADDED ***
        [ObservableProperty] // <<< MAKE SURE THIS IS PRESENT
        [NotifyCanExecuteChangedFor(nameof(AddPlatCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToPlatDetailCommand))]
        private bool _canManagePlats; // <<< Private field starts with _
        public PlatsViewModel(
            IPlatService platService,
            ICartService cartService,
            INavigationService navigationService,
            IAuthService authService) // *** ADDED ***
        {
            _platService = platService;
            _cartService = cartService;
            _navigationService = navigationService;
            _authService = authService; // *** ADDED ***
            Title = "Dishes";

            // *** ADDED: Listen for authentication changes ***
            _authService.PropertyChanged += AuthService_PropertyChanged;
            // *** ADDED: Set initial role status ***
            UpdateRolePermissions();
        }

        // *** ADDED: Handle Auth Changes ***
        private void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                UpdateRolePermissions();
            }
        }

        // *** ADDED: Update role property ***
        private void UpdateRolePermissions()
        {
            var user = _authService.CurrentUser;
            _canManagePlats = user != null && (user.Role == UserRole.Staff || user.Role == UserRole.Admin);
            // Debug.WriteLine($"PlatsViewModel: User Role = {user?.Role}, CanManagePlats = {CanManagePlats}");
        }

        [RelayCommand]
        private async Task LoadPlatsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            // Ensure role permissions are current before loading/displaying
            UpdateRolePermissions();
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

        // --- ADDED: CanExecute condition ---
        private bool CanAddPlat() => _canManagePlats && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanAddPlat))] // *** MODIFIED ***
        private async Task AddPlatAsync()
        {
            // Redundant check, CanExecute handles it, but safe:
            if (!_canManagePlats) return;
            await _navigationService.NavigateToAsync(nameof(PlatDetailPage));
        }

        // --- ADDED: CanExecute condition for editing ---
        private bool CanGoToDetail() => SelectedPlat != null && _canManagePlats && !IsBusy;

        // This command is now primarily for EDITING
        [RelayCommand(CanExecute = nameof(CanGoToDetail))] // *** MODIFIED ***
        private async Task GoToPlatDetailAsync()
        {
            // Redundant check, CanExecute handles it, but safe:
            if (SelectedPlat == null || !_canManagePlats) return;

            await _navigationService.NavigateToAsync(nameof(PlatDetailPage),
                new Dictionary<string, object>
                {
                    { "PlatId", SelectedPlat.Id }
                });
        }

        // No role check needed for adding to cart (assuming customers/all logged-in can do this)
        [RelayCommand]
        private async Task AddToCartAsync(Plat plat)
        {
            if (plat == null || IsBusy || _authService.CurrentUser == null) return;

            IsBusy = true;
            try
            {
                // Use the logged-in user's ID
                bool success = await _cartService.AddItemToCartAsync(_authService.CurrentUser.Id, plat.Id, 1);
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

        // TODO: Remember to unsubscribe from PropertyChanged if the ViewModel lifecycle requires it
        // public void Cleanup() { _authService.PropertyChanged -= AuthService_PropertyChanged; }
    }
}