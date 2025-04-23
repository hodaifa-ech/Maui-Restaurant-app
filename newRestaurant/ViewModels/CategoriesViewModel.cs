// ViewModels/CategoriesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.Views; // Needed for nameof page navigation
using System.Collections.ObjectModel;
using System.Diagnostics; // For Debug.WriteLine
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {

        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        private Category _selectedCategory; // Used for selection handling

        private readonly ICategoryService _categoryService; // ADD
        private readonly INavigationService _navigationService;

        public CategoriesViewModel(/*IDataService dataService,*/ ICategoryService categoryService, INavigationService navigationService) // CHANGE
        {
            // _dataService = dataService; // REMOVE
            _categoryService = categoryService; // ADD
            _navigationService = navigationService;
            Title = "Manage Categories";
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Categories.Clear();
                var categoriesList = await _categoryService.GetCategoriesAsync();
                foreach (var category in categoriesList)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading categories: {ex.Message}");
                // Optionally display an error message to the user
                await Shell.Current.DisplayAlert("Error", "Failed to load categories.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (IsBusy) return;
            // Navigate to detail page with no ID (indicating new category)
            await _navigationService.NavigateToAsync(nameof(CategoryDetailPage));
        }

        [RelayCommand]
        private async Task GoToCategoryDetailAsync()
        {
            if (IsBusy || SelectedCategory == null) return;

            // Navigate to detail page with the selected category's ID
            await _navigationService.NavigateToAsync(nameof(CategoryDetailPage),
                new Dictionary<string, object>
                {
                    { "CategoryId", SelectedCategory.Id } // Pass ID as parameter
                });

            // Deselect item after navigation (handled in OnAppearing of list page now)
            // SelectedCategory = null;
        }
    }
}