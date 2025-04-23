// ViewModels/ReservationsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class ReservationsViewModel : BaseViewModel
    {

        [ObservableProperty]
        private ObservableCollection<Reservation> _reservations = new();

        [ObservableProperty]
        private Reservation _selectedReservation;

        // TODO: Replace with actual logged-in user ID
        private const int CurrentUserId = 1;

        private readonly IReservationService _reservationService; // ADD
        private readonly INavigationService _navigationService;

        public ReservationsViewModel(/*IDataService dataService,*/ IReservationService reservationService, INavigationService navigationService) // CHANGE
        {
            // _dataService = dataService; // REMOVE
            _reservationService = reservationService; // ADD
            _navigationService = navigationService;
            Title = "Reservations";
        }

        [RelayCommand]
        private async Task LoadReservationsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Reservations.Clear();
                // Option 1: Load all reservations (Admin view)
                var reservationsList = await _reservationService.GetReservationsAsync();
                // Option 2: Load reservations for current user (Customer view)
                // var reservationsList = await _dataService.GetReservationsByUserAsync(CurrentUserId);

                foreach (var res in reservationsList)
                {
                    Reservations.Add(res);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading reservations: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load reservations.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddReservationAsync()
        {
            if (IsBusy) return;
            await _navigationService.NavigateToAsync(nameof(ReservationDetailPage));
        }

        [RelayCommand]
        private async Task GoToReservationDetailAsync()
        {
            if (IsBusy || SelectedReservation == null) return;

            await _navigationService.NavigateToAsync(nameof(ReservationDetailPage),
                new Dictionary<string, object>
                {
                    { "ReservationId", SelectedReservation.Id }
                });
        }
    }
}