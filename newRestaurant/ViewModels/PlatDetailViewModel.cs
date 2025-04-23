// ViewModels/PlatDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel; // Required for PropertyChangedEventArgs
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(PlatId), "PlatId")]
    public partial class PlatDetailViewModel : BaseViewModel
    {
        private int _platId;
        private bool _isInitialLoad = true;

        [ObservableProperty] private string _platName;
        [ObservableProperty] private string _platDescription;
        [ObservableProperty] private decimal _platPrice;
        [ObservableProperty] private Category _selectedCategory;
        [ObservableProperty] private ObservableCollection<Category> _categories = new();
        [ObservableProperty] private bool _isExistingPlat;

        // --- Role-based Property ---
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SavePlatCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeletePlatCommand))]
        private bool _canManagePlats; // True if Staff or Admin


        private readonly IPlatService _platService;
        private readonly ICategoryService _categoryService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService; // *** ADDED ***

        public int PlatId
        {
            get => _platId;
            set
            {
                SetProperty(ref _platId, value);
                IsExistingPlat = value > 0;
                _isInitialLoad = true; // Reset flag if ID changes
            }
        }

        public PlatDetailViewModel(
            IPlatService platService,
            ICategoryService categoryService,
            INavigationService navigationService,
            IAuthService authService) // *** ADDED ***
        {
            _platService = platService;
            _categoryService = categoryService;
            _navigationService = navigationService;
            _authService = authService; // *** ADDED ***
            Title = "Dish Details"; // Initial title

            // *** ADDED: Listen and set initial role ***
            _authService.PropertyChanged += AuthService_PropertyChanged;
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
            // Debug.WriteLine($"PlatDetailViewModel: User Role = {user?.Role}, CanManagePlats = {CanManagePlats}");
            // Maybe force UI update for fields if role changes while viewing?
            OnPropertyChanged(nameof(_canManagePlats));
        }

        public async Task InitializeAsync()
        {
            if (!_isInitialLoad || IsBusy) return;

            IsBusy = true;
            // Ensure role permissions are current before loading/displaying
            UpdateRolePermissions();
            try
            {
                Categories.Clear();
                var categoryList = await _categoryService.GetCategoriesAsync();
                foreach (var cat in categoryList) Categories.Add(cat);

                if (_platId > 0)
                {
                    // Title updated based on role later if needed
                    var plat = await _platService.GetPlatAsync(_platId);
                    if (plat != null)
                    {
                        PlatName = plat.Name;
                        PlatDescription = plat.Description;
                        PlatPrice = plat.Price;
                        SelectedCategory = Categories.FirstOrDefault(c => c.Id == plat.CategoryId);
                        IsExistingPlat = true;
                        Title = _canManagePlats ? $"Edit Dish: {plat.Name}" : $"View Dish: {plat.Name}"; // Title reflects action
                    }
                    else { /* Handle error */ await GoBackAsync(); return; }
                }
                else // New Dish - Should only be reachable by Staff/Admin due to navigation checks now
                {
                    Title = "Add New Dish";
                    PlatName = string.Empty;
                    PlatDescription = string.Empty;
                    PlatPrice = 0.0m;
                    SelectedCategory = Categories.FirstOrDefault();
                    IsExistingPlat = false;
                    // If somehow a non-staff user reaches here, prevent saving
                    if (!_canManagePlats)
                    {
                        await Shell.Current.DisplayAlert("Access Denied", "You do not have permission to add new dishes.", "OK");
                        await GoBackAsync();
                        return;
                    }
                }
                _isInitialLoad = false;
            }
            catch (Exception ex) { Debug.WriteLine($"Error initializing PlatDetailViewModel: {ex.Message}"); /* Handle error */ }
            finally { IsBusy = false; }
        }

        // --- ADDED: CanExecute condition ---
        private bool CanSaveChanges() => _canManagePlats && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanSaveChanges))] // *** MODIFIED ***
        private async Task SavePlatAsync()
        {
            // Redundant check, but safe:
            if (!_canManagePlats)
            {
                await Shell.Current.DisplayAlert("Access Denied", "You do not have permission to save dishes.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PlatName) || SelectedCategory == null || PlatPrice <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a name, select a category, and provide a valid price.", "OK");
                return;
            }

            IsBusy = true; // Set IsBusy inside the command execution
            bool success = false;
            try
            {
                Plat platToSave = new Plat
                {
                    Id = _platId,
                    Name = PlatName,
                    Description = PlatDescription,
                    Price = PlatPrice,
                    CategoryId = SelectedCategory.Id,
                };

                if (IsExistingPlat) success = await _platService.UpdatePlatAsync(platToSave);
                else success = await _platService.AddPlatAsync(platToSave);

                if (success) { await Shell.Current.DisplayAlert("Success", "Dish saved.", "OK"); await GoBackAsync(); }
                else { await Shell.Current.DisplayAlert("Error", "Failed to save dish.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error saving plat: {ex}"); /* Handle error */ }
            finally { IsBusy = false; }
        }

        // --- ADDED: CanExecute condition ---
        private bool CanDelete() => _canManagePlats && IsExistingPlat && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanDelete))] // *** MODIFIED ***
        private async Task DeletePlatAsync()
        {
            // Redundant check:
            if (!_canManagePlats || !IsExistingPlat) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", $"Delete '{PlatName}'?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true; // Set IsBusy inside the command execution
            try
            {
                bool success = await _platService.DeletePlatAsync(_platId);
                if (success) { await Shell.Current.DisplayAlert("Success", "Dish deleted.", "OK"); await GoBackAsync(); }
                else { await Shell.Current.DisplayAlert("Error", "Failed to delete dish.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error deleting dish: {ex.Message}"); /* Handle error */ }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await _navigationService.GoBackAsync();
        }

        // TODO: Remember to unsubscribe
        // public void Cleanup() { _authService.PropertyChanged -= AuthService_PropertyChanged; }
    }
}