// ViewModels/ReservationDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces; // Use interfaces
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // For PropertyChangedEventArgs
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(ReservationId), "ReservationId")]
    public partial class ReservationDetailViewModel : BaseViewModel
    {
        private int _reservationId;
        private bool _isInitialLoad = true;
        private int _reservationOwnerUserId = -1;

        [ObservableProperty] private Table _selectedTable;
        [ObservableProperty] private ObservableCollection<Table> _tables = new();
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private TimeSpan _startTime = DateTime.Now.TimeOfDay;
        [ObservableProperty] private TimeSpan _endTime = DateTime.Now.AddHours(1).TimeOfDay;
        [ObservableProperty] private ReservationStatus _status = ReservationStatus.Pending;
        [ObservableProperty] private string _userName = "Loading...";
        [ObservableProperty] private bool _isExistingReservation;
        [ObservableProperty] private bool _isStaffOrAdmin;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveReservationCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteReservationCommand))]
        private bool _canManageThisReservation;

        private readonly IAuthService _authService;
        private readonly IReservationService _reservationService;
        private readonly ITableService _tableService;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;

        public int ReservationId
        {
            get => _reservationId;
            set { SetProperty(ref _reservationId, value); IsExistingReservation = value > 0; _isInitialLoad = true; }
        }

        public ReservationDetailViewModel(
                                  IReservationService reservationService,
                                  ITableService tableService,
                                  IUserService userService,
                                  INavigationService navigationService,
                                  IAuthService authService)
        {
            _reservationService = reservationService;
            _tableService = tableService;
            _userService = userService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "Reservation Details";
            _authService.PropertyChanged += AuthService_PropertyChanged;
            UpdateBaseRolePermissions();
        }

        private void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                UpdateBaseRolePermissions();
                UpdateDetailedPermissions(_reservationOwnerUserId);
            }
        }

        private void UpdateBaseRolePermissions()
        {
            var user = _authService.CurrentUser;
            IsStaffOrAdmin = user != null && (user.Role == UserRole.Staff || user.Role == UserRole.Admin);
        }

        private void UpdateDetailedPermissions(int ownerUserId)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) CanManageThisReservation = false;
            else CanManageThisReservation = IsStaffOrAdmin || (ownerUserId > 0 && currentUser.Id == ownerUserId);
            SaveReservationCommand.NotifyCanExecuteChanged(); // Update command states
            DeleteReservationCommand.NotifyCanExecuteChanged();
        }


        public async Task InitializeAsync()
        {
            if (!_isInitialLoad || IsBusy) return;
            IsBusy = true;
            UpdateBaseRolePermissions();
            _reservationOwnerUserId = -1;
            CanManageThisReservation = false; // Default until loaded

            try
            {
                Tables.Clear();
                var tableList = await _tableService.GetTablesAsync();
                foreach (var table in tableList) Tables.Add(table);

                if (_reservationId > 0) // Existing
                {
                    var reservation = await _reservationService.GetReservationAsync(_reservationId);
                    if (reservation != null)
                    {
                        _reservationOwnerUserId = reservation.UserId;
                        UpdateDetailedPermissions(_reservationOwnerUserId);
                        SelectedTable = Tables.FirstOrDefault(t => t.Id == reservation.TableId);
                        SelectedDate = reservation.TimeStart.Date; StartTime = reservation.TimeStart.TimeOfDay; EndTime = reservation.TimeEnd.TimeOfDay;
                        Status = reservation.Status; UserName = reservation.User?.Username ?? "Unknown"; IsExistingReservation = true;
                        Title = CanManageThisReservation ? "Edit Reservation" : "View Reservation";
                    }
                    else { await Shell.Current.DisplayAlert("Error", "Reservation not found.", "OK"); await GoBackAsync(); return; }
                }
                else // New
                {
                    var currentUser = _authService.CurrentUser;
                    if (currentUser == null) { await Shell.Current.DisplayAlert("Error", "Not logged in.", "OK"); await GoBackAsync(); return; }
                    _reservationOwnerUserId = currentUser.Id; UpdateDetailedPermissions(_reservationOwnerUserId);
                    Title = "Add New Reservation"; SelectedTable = Tables.FirstOrDefault();
                    SelectedDate = DateTime.Today.AddDays(1); StartTime = TimeSpan.FromHours(18); EndTime = StartTime.Add(TimeSpan.FromHours(2));
                    Status = ReservationStatus.Pending; IsExistingReservation = false; UserName = currentUser.Username;
                    if (!CanManageThisReservation) { await Shell.Current.DisplayAlert("Error", "Cannot create reservation.", "OK"); await GoBackAsync(); return; }
                }
                _isInitialLoad = false;
            }
            catch (Exception ex) { Debug.WriteLine($"Error initializing ReservationDetailViewModel: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        private bool CanSave() => CanManageThisReservation && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveReservationAsync()
        {
            if (!CanManageThisReservation) return;
            var currentUserId = _authService.CurrentUser?.Id;
            int userIdToSave = IsExistingReservation ? _reservationOwnerUserId : currentUserId ?? -1;
            if (userIdToSave <= 0) { await Shell.Current.DisplayAlert("Error", "User info missing.", "OK"); return; }
            DateTime startDateTime = SelectedDate.Date + StartTime; DateTime endDateTime = SelectedDate.Date + EndTime;
            if (SelectedTable == null || endDateTime <= startDateTime || (startDateTime < DateTime.Now && !IsExistingReservation)) { await Shell.Current.DisplayAlert("Validation Error", "Invalid table or times.", "OK"); return; }
            IsBusy = true; bool success = false;
            try
            {
                Reservation reservationToSave = new Reservation
                {
                    Id = _reservationId,
                    TableId = SelectedTable.Id,
                    UserId = userIdToSave,
                    TimeStart = startDateTime,
                    TimeEnd = endDateTime,
                    Status = (IsExistingReservation && IsStaffOrAdmin) ? Status : (IsExistingReservation ? (await _reservationService.GetReservationAsync(_reservationId)).Status : ReservationStatus.Pending)
                };
                if (IsExistingReservation) success = await _reservationService.UpdateReservationAsync(reservationToSave);
                else success = await _reservationService.AddReservationAsync(reservationToSave);
                if (success) { await Shell.Current.DisplayAlert("Success", "Reservation saved.", "OK"); await GoBackAsync(); }
                else { await Shell.Current.DisplayAlert("Error", "Failed to save reservation (Overlap?).", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error saving reservation: {ex}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        private bool CanDelete() => CanManageThisReservation && IsExistingReservation && !IsBusy;
        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task DeleteReservationAsync()
        {
            if (!CanManageThisReservation || !IsExistingReservation) return;
            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", "Delete this reservation?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                bool success = await _reservationService.DeleteReservationAsync(_reservationId);
                if (success) { await Shell.Current.DisplayAlert("Success", "Reservation deleted.", "OK"); await GoBackAsync(); }
                else { await Shell.Current.DisplayAlert("Error", "Failed to delete reservation.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error deleting reservation: {ex.Message}"); /* Handle */ }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task GoBackAsync() { if (IsBusy) return; await _navigationService.GoBackAsync(); }
        // TODO: Unsubscribe _authService.PropertyChanged -= AuthService_PropertyChanged;
    }
}