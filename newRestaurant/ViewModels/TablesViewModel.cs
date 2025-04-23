using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace newRestaurant.ViewModels;

public partial class TablesViewModel : BaseViewModel
{
	
        [ObservableProperty]
        private ObservableCollection<Table> _tables = new();

        [ObservableProperty]
        private Table _selectedTable; // Used for selection handling

        private readonly ITableService _tableService; // ADD
        private readonly INavigationService _navigationService;

        public TablesViewModel( ITableService tableService, INavigationService navigationService) 
        {
            _tableService = tableService; // ADD
            _navigationService = navigationService;
            Title = "Manage Tables";
        }

        [RelayCommand]
        private async Task LoadTablesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                _tables.Clear();
                var tablesList = await _tableService.GetTablesAsync();
                foreach (var table in tablesList)
                {
                    _tables.Add(table);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading tablesList: {ex.Message}");
                // Optionally display an error message to the user
                await Shell.Current.DisplayAlert("Error", "Failed to load tablesList.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddTableAsync()
        {
            if (IsBusy) return;
            // Navigate to detail page with no ID (indicating new category)
            await _navigationService.NavigateToAsync(nameof(TableDetailPage));
        }

        [RelayCommand]
        private async Task GoToTableDetailAsync()
        {
            if (IsBusy || SelectedTable == null) return;

            // Navigate to detail page with the selected category's ID
            await _navigationService.NavigateToAsync(nameof(TableDetailPage),
                new Dictionary<string, object>
                {
                        { "TableId", SelectedTable.Id } // Pass ID as parameter
                });

            // Deselect item after navigation (handled in OnAppearing of list page now)
            // SelectedCategory = null;
        }

}