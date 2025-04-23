// ViewModels/TableDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System;
using System.ComponentModel; // For PropertyChangedEventArgs
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
        [ObservableProperty] private int _capacity = 4;
        [ObservableProperty] private bool _isExistingTable;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveTableCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteTableCommand))]
        private bool _canManageTables;

        public int TableId
        {
            get => _tableId;
            set { SetProperty(ref _tableId, value); IsExistingTable = value > 0; _isInitialLoad = true; }
        }

        private readonly ITableService _tableService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public TableDetailViewModel(
            ITableService tableService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _tableService = tableService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "Table Details";
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
            CanManageTables = user != null && (user.Role == UserRole.Staff || user.Role == UserRole.Admin);
            SaveTableCommand.NotifyCanExecuteChanged();
            DeleteTableCommand.NotifyCanExecuteChanged();
        }

        public async Task InitializeAsync()
        {
            if (!_isInitialLoad || IsBusy) return;
            IsBusy = true;
            UpdateRolePermissions();

            if (!CanManageTables && _tableId == 0)
            {
                await Shell.Current.DisplayAlert("Access Denied", "You cannot add tables.", "OK");
                await GoBackAsync(); IsBusy = false; return;
            }

            try
            {
                if (_tableId > 0)
                {
                    Title = CanManageTables ? "Edit Table" : "View Table";
                    var table = await _tableService.GetTableAsync(_tableId);
                    if (table != null) { TableNumber = table.TableNumber; Capacity = table.Capacity; IsExistingTable = true; }
                    else { await Shell.Current.DisplayAlert("Error", "Table not found.", "OK"); await GoBackAsync(); return; }
                }
                else { Title = "Add New Table"; TableNumber = string.Empty; Capacity = 4; IsExistingTable = false; }
                _isInitialLoad = false;
            }
            catch (Exception ex) { Debug.WriteLine($"Error initializing TableDetailViewModel: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        private bool CanSaveTable() => CanManageTables && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanSaveTable))]
        private async Task SaveTableAsync()
        {
            if (!CanManageTables) return;
            if (string.IsNullOrWhiteSpace(TableNumber) || Capacity <= 0) { await Shell.Current.DisplayAlert("Validation Error", "Valid Number & Capacity required.", "OK"); return; }
            IsBusy = true; bool success = false;
            try
            {
                Table tableToSave = new Table { Id = _tableId, TableNumber = TableNumber.Trim(), Capacity = Capacity };
                if (IsExistingTable) success = await _tableService.UpdateTableAsync(tableToSave);
                else success = await _tableService.AddTableAsync(tableToSave);
                if (success) { await Shell.Current.DisplayAlert("Success", "Table saved.", "OK"); await GoBackAsync(); }
                else { await Shell.Current.DisplayAlert("Error", "Failed to save table (Number exists?).", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error saving table: {ex}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        private bool CanDeleteTable() => CanManageTables && IsExistingTable && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanDeleteTable))]
        private async Task DeleteTableAsync()
        {
            if (!CanManageTables || !IsExistingTable) return;
            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", $"Delete Table '{TableNumber}'?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                // Service layer shows alert on failure (e.g., active reservations)
                bool success = await _tableService.DeleteTableAsync(_tableId);
                if (success) { await Shell.Current.DisplayAlert("Success", "Table deleted.", "OK"); await GoBackAsync(); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error deleting table: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoBackAsync() { if (IsBusy) return; await _navigationService.GoBackAsync(); }
        // TODO: Unsubscribe _authService.PropertyChanged -= AuthService_PropertyChanged;
    }
}