// ViewModels/CategoryDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    // This attribute links the QueryProperty "CategoryId" from navigation parameters
    // to the CategoryId property in this ViewModel.
    [QueryProperty(nameof(CategoryId), "CategoryId")]
    public partial class CategoryDetailViewModel : BaseViewModel
    {
       
        private int _categoryId; // Backing field for the query property

        [ObservableProperty]
        private string _categoryName;

        [ObservableProperty]
        private bool _isExistingCategory; // To control UI elements like Delete button

        // This property receives the value from the navigation parameter
        public int CategoryId
        {
            get => _categoryId;
            set
            {
                SetProperty(ref _categoryId, value);
                IsExistingCategory = value > 0; // Set flag based on ID
                if (value > 0)
                {
                    // Load existing category data when ID is set
                    LoadCategoryAsync(value);
                }
                else
                {
                    // Prepare for new category entry
                    Title = "Add New Category";
                    CategoryName = string.Empty;
                }
            }
        }

        private readonly ICategoryService _categoryService; // ADD
        private readonly INavigationService _navigationService;

        public CategoryDetailViewModel(/*IDataService dataService,*/ ICategoryService categoryService, INavigationService navigationService) // CHANGE
        {
            // _categoryService = dataService; // REMOVE
            _categoryService = categoryService; // ADD
            _navigationService = navigationService;
            Title = "Category Details";
        }

        private async Task LoadCategoryAsync(int categoryId)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var category = await _categoryService.GetCategoryAsync(categoryId);
                if (category != null)
                {
                    CategoryName = category.Name;
                    Title = $"Edit: {category.Name}";
                    IsExistingCategory = true;
                }
                else
                {
                    Title = "Add New Category";
                    IsExistingCategory = false;
                    await Shell.Current.DisplayAlert("Error", "Category not found.", "OK");
                    await GoBackAsync(); // Go back if category not found
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading category: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load category.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveCategoryAsync()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Category name cannot be empty.", "OK");
                return;
            }

            IsBusy = true;
            bool success = false;
            try
            {
                Category category = new Category
                {
                    Id = CategoryId, // If 0, it's new; otherwise, it's an update
                    Name = CategoryName
                };

                if (IsExistingCategory) // Update
                {
                    success = await _categoryService.UpdateCategoryAsync(category);
                }
                else // Add New
                {
                    // Ensure Id is 0 for adding
                    category.Id = 0;
                    success = await _categoryService.AddCategoryAsync(category);
                }

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Category saved successfully.", "OK");
                    await GoBackAsync(); // Go back to the list page
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to save category.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving category: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync()
        {
            if (IsBusy || !IsExistingCategory) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", $"Are you sure you want to delete the category '{CategoryName}'?", "Yes", "No");
            if (!confirm) return;


            IsBusy = true;
            try
            {
                // TODO: Add check if category is in use by Plats before deleting
                bool success = await _categoryService.DeleteCategoryAsync(CategoryId);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Category deleted.", "OK");
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete category.", "OK");
                }
            }
            catch (Exception ex) // Catch potential FK constraint errors if not handled in service
            {
                Debug.WriteLine($"Error deleting category: {ex.Message}");
                // Check for specific database exceptions if needed
                await Shell.Current.DisplayAlert("Error", $"Could not delete category. It might be in use. ({ex.Message})", "OK");
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