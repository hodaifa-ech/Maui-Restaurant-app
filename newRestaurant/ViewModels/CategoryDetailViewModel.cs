// ViewModels/CategoryDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces; // Use Interfaces
using System.ComponentModel; // For PropertyChangedEventArgs
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(CategoryId), "CategoryId")]
    public partial class CategoryDetailViewModel : BaseViewModel
    {
        private int _categoryId;
        private bool _isInitialLoad = true;

        [ObservableProperty] private string _categoryName;
        [ObservableProperty] private bool _isExistingCategory;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCategoryCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCategoryCommand))]
        private bool _canManageCategories;

        public int CategoryId
        {
            get => _categoryId;
            set { SetProperty(ref _categoryId, value); IsExistingCategory = value > 0; _isInitialLoad = true; }
        }

        private readonly ICategoryService _categoryService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public CategoryDetailViewModel(
            ICategoryService categoryService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _categoryService = categoryService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "Category Details";
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
            SaveCategoryCommand.NotifyCanExecuteChanged();
            DeleteCategoryCommand.NotifyCanExecuteChanged();
        }

        public async Task InitializeAsync()
        {
            if (!_isInitialLoad || IsBusy) return;
            IsBusy = true;
            UpdateRolePermissions();

            if (_categoryId == 0 && !_canManageCategories)
            {
                await Shell.Current.DisplayAlert("Access Denied", "You cannot add categories.", "OK");
                await GoBackAsync(); IsBusy = false; return;
            }

            try
            {
                if (_categoryId > 0)
                {
                    var category = await _categoryService.GetCategoryAsync(_categoryId);
                    if (category != null)
                    {
                        CategoryName = category.Name; IsExistingCategory = true;
                        Title = _canManageCategories ? $"Edit: {category.Name}" : $"View: {category.Name}";
                    }
                    else { await Shell.Current.DisplayAlert("Error", "Category not found.", "OK"); await GoBackAsync(); return; }
                }
                else { Title = "Add New Category"; CategoryName = string.Empty; IsExistingCategory = false; }
                _isInitialLoad = false;
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading category: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        private bool CanSaveCategory() => _canManageCategories && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanSaveCategory))]
        private async Task SaveCategoryAsync()
        {
            if (!_canManageCategories) return;
            if (string.IsNullOrWhiteSpace(CategoryName)) { await Shell.Current.DisplayAlert("Validation Error", "Name required.", "OK"); return; }
            IsBusy = true; bool success = false;
            try
            {
                Category category = new Category { Id = CategoryId, Name = CategoryName };
                if (IsExistingCategory) success = await _categoryService.UpdateCategoryAsync(category);
                else success = await _categoryService.AddCategoryAsync(category);
                if (success) { await Shell.Current.DisplayAlert("Success", "Category saved.", "OK"); await GoBackAsync(); }
                else { await Shell.Current.DisplayAlert("Error", "Failed to save category.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error saving category: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        private bool CanDeleteCategory() => _canManageCategories && IsExistingCategory && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanDeleteCategory))]
        private async Task DeleteCategoryAsync()
        {
            if (!_canManageCategories || !IsExistingCategory) return;
            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", $"Delete '{CategoryName}'?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                bool success = await _categoryService.DeleteCategoryAsync(CategoryId);
                // Service layer might show alert on failure (e.g., FK constraint)
                if (success) { await Shell.Current.DisplayAlert("Success", "Category deleted.", "OK"); await GoBackAsync(); }
                // else { await Shell.Current.DisplayAlert("Error", "Failed to delete category.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error deleting category: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoBackAsync() { if (IsBusy) return; await _navigationService.GoBackAsync(); }
        // TODO: Unsubscribe _authService.PropertyChanged -= AuthService_PropertyChanged;
    }
}