// ViewModels/ReservationDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;

// using newRestaurant.Services; // Not needed if using interfaces only
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(ReservationId), "ReservationId")] // Use property name for QueryProperty
    public partial class ReservationDetailViewModel : BaseViewModel
    {
        private int _reservationId;
        private bool _isInitialLoad = true; // Flag for OnAppearing

        [ObservableProperty] private Table _selectedTable;
        [ObservableProperty] private ObservableCollection<Table> _tables = new();
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private TimeSpan _startTime = DateTime.Now.TimeOfDay;
        [ObservableProperty] private TimeSpan _endTime = DateTime.Now.AddHours(1).TimeOfDay;
        [ObservableProperty] private ReservationStatus _status = ReservationStatus.Pending;
        [ObservableProperty] private string _userName = "Loading...";
        [ObservableProperty] private bool _isExistingReservation;

        private readonly IAuthService _authService;
        private readonly IReservationService _reservationService;
        private readonly ITableService _tableService;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;

        // Public property for QueryProperty binding
        public int ReservationId
        {
            get => _reservationId;
            set
            {
                SetProperty(ref _reservationId, value);
                IsExistingReservation = value > 0;
                // Reset flag so InitializeAsync runs again if ID changes after appearing
                _isInitialLoad = true;
            }
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
        }

        // Renamed from LoadDependenciesAsync - Called from Page's OnAppearing
        public async Task InitializeAsync()
        {
            // Prevent multiple loads per appearance/ID change
            if (!_isInitialLoad || IsBusy) return;

            IsBusy = true;
            try
            {
                // --- Always Load Tables for Picker ---
                Tables.Clear(); // Clear before adding
                var tableList = await _tableService.GetTablesAsync();
                foreach (var table in tableList)
                {
                    Tables.Add(table);
                }

                // --- Load Reservation Details or Set Defaults ---
                if (_reservationId > 0)
                {
                    Title = "Edit Reservation";
                    var reservation = await _reservationService.GetReservationAsync(_reservationId);
                    if (reservation != null)
                    {
                        SelectedTable = Tables.FirstOrDefault(t => t.Id == reservation.TableId);
                        SelectedDate = reservation.TimeStart.Date;
                        StartTime = reservation.TimeStart.TimeOfDay;
                        EndTime = reservation.TimeEnd.TimeOfDay;
                        Status = reservation.Status;
                        UserName = reservation.User?.Username ?? "Unknown";
                        IsExistingReservation = true; // Redundant but safe
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Reservation not found.", "OK");
                        await GoBackAsync();
                        return; // Exit if not found
                    }
                }
                else // New Reservation
                {
                    Title = "Add New Reservation";
                    SelectedTable = Tables.FirstOrDefault(); // Set default AFTER loading Tables
                    SelectedDate = DateTime.Today;
                    StartTime = TimeSpan.FromHours(DateTime.Now.Hour + 1);
                    EndTime = StartTime.Add(TimeSpan.FromHours(1));
                    Status = ReservationStatus.Pending;
                    IsExistingReservation = false;
                    var currentUser = _authService.CurrentUser;
                    UserName = currentUser?.Username ?? "Error: Not Logged In";
                }

                _isInitialLoad = false; // Mark load as complete for this instance/ID
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ReservationDetailViewModel: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load reservation details.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // --- SaveReservationAsync, DeleteReservationAsync, GoBackAsync ---
        // (These methods remain largely the same, ensure they use _reservationId field)
        [RelayCommand]
        private async Task SaveReservationAsync()
        {
            var currentUserId = _authService.CurrentUser?.Id;
            if (currentUserId == null)
            {
                await Shell.Current.DisplayAlert("Error", "You must be logged in.", "OK");
                return;
            }
            if (IsBusy) return;

            DateTime startDateTime = SelectedDate.Date + StartTime;
            DateTime endDateTime = SelectedDate.Date + EndTime;

            if (SelectedTable == null) { /* Validation */ return; }
            if (endDateTime <= startDateTime) { /* Validation */ return; }
            if (startDateTime < DateTime.Now && _reservationId == 0) { /* Validation */ return; }

            IsBusy = true;
            bool success = false;
            try
            {
                // Create a NEW object for saving, especially important if updating
                Reservation reservationToSave = new Reservation
                {
                    Id = _reservationId,
                    TableId = SelectedTable.Id,
                    UserId = currentUserId.Value,
                    TimeStart = startDateTime,
                    TimeEnd = endDateTime,
                    Status = IsExistingReservation ? Status : ReservationStatus.Pending
                };

                if (IsExistingReservation)
                {
                    // Ensure UpdateReservationAsync handles finding/updating correctly
                    success = await _reservationService.UpdateReservationAsync(reservationToSave);
                }
                else
                {
                    reservationToSave.Id = 0; // Ensure 0 for add
                    success = await _reservationService.AddReservationAsync(reservationToSave);
                }

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Reservation saved.", "OK");
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to save reservation. Check conflicts/logs.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving reservation: {ex}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteReservationAsync()
        {
            if (IsBusy || !IsExistingReservation) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirm Delete", "Delete this reservation?", "Yes", "No");
            if (!confirm) return;

            IsBusy = true;
            try
            {
                bool success = await _reservationService.DeleteReservationAsync(_reservationId);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Reservation deleted.", "OK");
                    await GoBackAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete reservation.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting reservation: {ex.Message}");
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
            await _navigationService.GoBackAsync();
        }
    }
}