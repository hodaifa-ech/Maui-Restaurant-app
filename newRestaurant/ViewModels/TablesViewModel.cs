// ViewModels/TablesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces; // Ensure interfaces are used
using newRestaurant.Views;
using System.Collections.ObjectModel;
using System.ComponentModel; // For PropertyChangedEventArgs
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class TablesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Table> _tables = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToTableDetailCommand))]
        private Table _selectedTable;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddTableCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToTableDetailCommand))]
        private bool _canManageTables;

        private readonly ITableService _tableService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public TablesViewModel(
            ITableService tableService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _tableService = tableService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "Tables"; // Default title

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
            Title = CanManageTables ? "Manage Tables" : "View Tables"; // Adjust title
        }

        [RelayCommand]
        private async Task LoadTablesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            UpdateRolePermissions();
            try
            {
                _tables.Clear();
                var tablesList = await _tableService.GetTablesAsync();
                foreach (var table in tablesList) _tables.Add(table);
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading tables: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Failed to load tables.", "OK"); }
            finally { IsBusy = false; }
        }

        private bool CanAddTable() => CanManageTables && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanAddTable))]
        private async Task AddTableAsync()
        {
            if (!CanManageTables) return;
            await _navigationService.NavigateToAsync(nameof(TableDetailPage));
        }

        private bool CanGoToDetail() => CanManageTables && SelectedTable != null && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanGoToDetail))]
        private async Task GoToTableDetailAsync()
        {
            if (!CanManageTables || SelectedTable == null) return;
            await _navigationService.NavigateToAsync(nameof(TableDetailPage), new Dictionary<string, object> { { "TableId", SelectedTable.Id } });
        }
        // TODO: Unsubscribe _authService.PropertyChanged -= AuthService_PropertyChanged;
    }
}