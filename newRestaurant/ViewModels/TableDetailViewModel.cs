// ViewModels/TableDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(TableId), "TableId")]
    public partial class TableDetailViewModel : BaseViewModel
    {
        private int _tableId;
        private bool _isInitialLoad = true;

        [ObservableProperty] private string _tableNumber;
        [ObservableProperty] private int _capacity = 4; // Default capacity
        [ObservableProperty] private bool _isExistingTable;

        private readonly ITableService _tableService;
        private readonly INavigationService _navigationService;

        // Public property for QueryProperty binding
        public int TableId
        {
            get => _tableId;
            set
            {
                SetProperty(ref _tableId, value);
                IsExistingTable = value > 0;
                _isInitialLoad = true; // Reset flag if ID changes after first appearance
            }
        }

        public TableDetailViewModel(ITableService tableService, INavigationService navigationService)
        {
            _tableService = tableService;
            _navigationService = navigationService;
            Title = "Table Details";
        }

        // Called from Page's OnAppearing
        public async Task InitializeAsync()
        {
            if (!_isInitialLoad || IsBusy) return;

            IsBusy = true;
            try
            {
                if (_tableId > 0)
                {
                    Title = "Edit Table";
                    var table = await _tableService.GetTableAsync(_tableId);
                    if (table != null)
                    {
                        TableNumber = table.TableNumber;
                        Capacity = table.Capacity;
                        IsExistingTable = true; // Redundant but safe
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Table not found.", "OK");
                        await GoBackAsync();
                        return; // Exit if not found
                    }
                }
                else // New Table
                {
                    Title = "Add New Table";
                    TableNumber = string.Empty;
                    Capacity = 4; // Reset to default
                    IsExistingTable = false;
                }
                _isInitialLoad = false; // Mark load as complete
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing TableDetailViewModel: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load table details.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveTableAsync()
        {
            if (IsBusy) return;

            // Validation
            if (string.IsNullOrWhiteSpace(TableNumber))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Table Number cannot be empty.", "OK");
                return;
            }
            if (Capacity <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Capacity must be greater than zero.", "OK");
                return;
            }

            IsBusy = true;
            bool success = false;
            try
            {
                // Create a NEW object for saving/updating
                Table tableToSave = new Table
                {
                    Id = _tableId, // Use the backing field
                    TableNumber = TableNumber.Trim(), // Trim whitespace
                    Capacity = Capacity
                };

                if (IsExistingTable)
                {
                    success = await _tableService.UpdateTableAsync(tableToSave);
                }
                else
                {
                    tableToSave.Id = 0; // Ensure 0 for add
                    success = await _tableService.AddTableAsync(tableToSave);
                }

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Table saved successfully.", "OK");
                    await GoBackAsync();
                }
                else
                {
                    // Service layer might provide more specific errors (e.g., duplicate number)
                    await Shell.Current.DisplayAlert("Error", "Failed to save table. Check if the Table Number already exists.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving table: {ex}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteTableAsync()
        {
            if (IsBusy || !IsExistingTable) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", $"Delete Table '{TableNumber}'? This cannot be done if it has active reservations.", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                // DeleteTableAsync in service now contains the check for reservations
                bool success = await _tableService.DeleteTableAsync(_tableId);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Table deleted.", "OK");
                    await GoBackAsync();
                }
                // If !success, the service layer likely showed an alert already
                // else
                // {
                //    await Shell.Current.DisplayAlert("Error", "Failed to delete table (see console/logs).", "OK");
                // }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting table: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            if (IsBusy) return; // Prevent double navigation
            await _navigationService.GoBackAsync();
        }
    }
}