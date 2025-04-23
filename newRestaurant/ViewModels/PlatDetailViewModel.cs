// using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(PlatId), "PlatId")]
    public partial class PlatDetailViewModel : BaseViewModel
    {
        bool _dependenciesLoaded = false;
        private int _platId;
        private bool _isInitialLoad = true; // Flag for OnAppearing

        [ObservableProperty] private string _platName;
        [ObservableProperty] private string _platDescription;
        [ObservableProperty] private decimal _platPrice;
        [ObservableProperty] private Category _selectedCategory;
        [ObservableProperty] private ObservableCollection<Category> _categories = new();
        [ObservableProperty] private bool _isExistingPlat;

        private readonly IPlatService _platService;
        private readonly ICategoryService _categoryService;
        private readonly INavigationService _navigationService;

        public int PlatId
        {
            get => _platId;
            set
            {
                // Set property but defer loading to OnAppearing or explicit call
                SetProperty(ref _platId, value);
                IsExistingPlat = value > 0;
                // Reset initial load flag if ID changes after first appearance
                _isInitialLoad = true;
            }
        }

        public PlatDetailViewModel(IPlatService platService, ICategoryService categoryService, INavigationService navigationService)
        {
            _platService = platService;
            _categoryService = categoryService;
            _navigationService = navigationService;
            Title = "Dish Details";
        }

        // --- NEW Method to call from OnAppearing ---
        public async Task InitializeAsync()
        {
            // Only run the full load logic once per instance or when PlatId changes
            if (!_isInitialLoad) return;
            if (IsBusy) return; // Prevent re-entrancy if already loading

            IsBusy = true;
            try
            {
                // --- Load Categories ---
                Categories.Clear(); // Clear before adding
                var categoryList = await _categoryService.GetCategoriesAsync();
                foreach (var cat in categoryList)
                {
                    Categories.Add(cat);
                }

                // --- Load Plat or Set Defaults ---
                if (_platId > 0)
                {
                    Title = "Edit Dish";
                    var plat = await _platService.GetPlatAsync(_platId);
                    if (plat != null)
                    {
                        PlatName = plat.Name;
                        PlatDescription = plat.Description;
                        PlatPrice = plat.Price;
                        SelectedCategory = Categories.FirstOrDefault(c => c.Id == plat.CategoryId);
                        IsExistingPlat = true; // Redundant but safe
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Dish not found.", "OK");
                        await GoBackAsync();
                        return; // Exit if plat not found
                    }
                }
                else // New Dish
                {
                    Title = "Add New Dish";
                    PlatName = string.Empty;
                    PlatDescription = string.Empty;
                    PlatPrice = 0.0m;
                    SelectedCategory = Categories.FirstOrDefault(); // Set default selection AFTER loading categories
                    IsExistingPlat = false;
                }

                _isInitialLoad = false; // Mark initial load as complete
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing PlatDetailViewModel: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load dish details.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Combined loading method
        public async Task LoadDataBasedOnIdAsync()
        {
            // Prevent re-entry and multiple loads
            if (IsBusy || _dependenciesLoaded) return;

            IsBusy = true;
            try
            {
                // --- Always load categories ---
                Categories.Clear();
                var categoryList = await _categoryService.GetCategoriesAsync();
                foreach (var cat in categoryList)
                {
                    Categories.Add(cat);
                }
                _dependenciesLoaded = true; // Mark dependencies as loaded

                // --- Load Plat specific data only if ID exists ---
                if (_platId > 0)
                {
                    Title = "Edit Dish";
                    var plat = await _platService.GetPlatAsync(_platId);
                    if (plat != null)
                    {
                        PlatName = plat.Name;
                        PlatDescription = plat.Description;
                        PlatPrice = plat.Price;
                        // Find and set the selected category in the Picker
                        SelectedCategory = Categories.FirstOrDefault(c => c.Id == plat.CategoryId);
                        // Ensure IsExistingPlat is correctly set (it should be already by setter)
                        IsExistingPlat = true;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Dish not found.", "OK");
                        await GoBackAsync();
                    }
                }
                else // New Dish
                {
                    Title = "Add New Dish";
                    PlatName = string.Empty;
                    PlatDescription = string.Empty;
                    PlatPrice = 0.0m;
                    // Now Categories are loaded, so this is safe
                    SelectedCategory = Categories.FirstOrDefault();
                    IsExistingPlat = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading dish details/categories: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load details.", "OK");
                // Optionally GoBackAsync() on critical load failure
            }
            finally
            {
                IsBusy = false;
            }
        }



        [RelayCommand]
        private async Task SavePlatAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(PlatName) || SelectedCategory == null || PlatPrice <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a name, select a category, and provide a valid price.", "OK");
                return;
            }

            IsBusy = true;
            bool success = false;
            try
            {
                // **IMPORTANT FIX FOR PROBLEM 2**
                // Create a NEW Plat object for saving, don't reuse the one potentially loaded
                Plat platToSave = new Plat
                {
                    Id = _platId, // Set the ID for update or 0 for add
                    Name = PlatName,
                    Description = PlatDescription,
                    Price = PlatPrice,
                    CategoryId = SelectedCategory.Id,
                    // DO NOT set the Category navigation property here if updating
                    // Category = SelectedCategory // Avoid this for updates
                };


                if (IsExistingPlat)
                {
                    // Let EF Core handle tracking based on the ID
                    success = await _platService.UpdatePlatAsync(platToSave);
                }
                else
                {
                    platToSave.Id = 0; // Explicitly ensure 0 for add
                    success = await _platService.AddPlatAsync(platToSave);
                }

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Dish saved successfully.", "OK");
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to save dish. Check logs.", "OK");
                }
            }
            catch (Exception ex)
            {
                // The InvalidOperationException about tracking might be caught here
                Debug.WriteLine($"Error saving plat: {ex}"); // Log full exception
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }





        [RelayCommand]
        private async Task DeletePlatAsync()
        {
            if (IsBusy || !IsExistingPlat) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", $"Are you sure you want to delete '{PlatName}'?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                bool success = await _platService.DeletePlatAsync(_platId);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Dish deleted.", "OK");
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete dish.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting dish: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Could not delete dish. It might be referenced elsewhere. ({ex.Message})", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await _navigationService.GoBackAsync();
        }

    }
}