// ViewModels/CategoriesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces; // Use interfaces
using newRestaurant.Views;
using System.Collections.ObjectModel;
using System.ComponentModel; // For PropertyChangedEventArgs
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToCategoryDetailCommand))]
        private Category _selectedCategory;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCategoryCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToCategoryDetailCommand))]
        private bool _canManageCategories;

        private readonly ICategoryService _categoryService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public CategoriesViewModel(
            ICategoryService categoryService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _categoryService = categoryService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "Categories"; // Default title

            _authService.PropertyChanged += AuthService_PropertyChanged;
            UpdateRolePermissions();
        }

        private void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser)) UpdateRolePermissions();
        }

        private void UpdateRolePermissions()
        {
            var user = _authService.CurrentUser;
            _canManageCategories = user != null && (user.Role == UserRole.Staff || user.Role == UserRole.Admin);
            Title = _canManageCategories ? "Manage Categories" : "View Categories"; // Adjust title
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            UpdateRolePermissions();
            try
            {
                Categories.Clear();
                var categoriesList = await _categoryService.GetCategoriesAsync();
                foreach (var category in categoriesList) Categories.Add(category);
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading categories: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Failed to load categories.", "OK"); }
            finally { IsBusy = false; }
        }

        private bool CanAddCategory() => _canManageCategories && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanAddCategory))]
        private async Task AddCategoryAsync()
        {
            if (!_canManageCategories) return;
            await _navigationService.NavigateToAsync(nameof(CategoryDetailPage));
        }

        private bool CanGoToDetail() => _canManageCategories && SelectedCategory != null && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanGoToDetail))]
        private async Task GoToCategoryDetailAsync()
        {
            if (!_canManageCategories || SelectedCategory == null) return;
            await _navigationService.NavigateToAsync(nameof(CategoryDetailPage), new Dictionary<string, object> { { "CategoryId", SelectedCategory.Id } });
        }
        // TODO: Unsubscribe _authService.PropertyChanged -= AuthService_PropertyChanged;
    }
}