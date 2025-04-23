// ViewModels/ReservationsViewModel.cs
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
    public partial class ReservationsViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Reservation> _reservations = new();

        [ObservableProperty]
        private Reservation _selectedReservation;

        [ObservableProperty]
        private bool _isStaffOrAdmin;
        [ObservableProperty]
     // Use generated property name
        private bool _canAddReservation;

        private readonly IReservationService _reservationService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public ReservationsViewModel(
            IReservationService reservationService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _reservationService = reservationService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "Reservations";

            _authService.PropertyChanged += AuthService_PropertyChanged;
            UpdateRolePermissions();
        }

        private void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                UpdateRolePermissions();
                // Reload list when user changes
                LoadReservationsCommand.ExecuteAsync(null);
            }
        }

        private void UpdateRolePermissions()
        {
            var user = _authService.CurrentUser;
            IsStaffOrAdmin = user != null && (user.Role == UserRole.Staff || user.Role == UserRole.Admin);
            CanAddReservation = user != null; // Any logged-in user can initiate adding
             // Update command state
        }

        [RelayCommand]
        private async Task LoadReservationsAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) { Reservations.Clear(); return; }
            if (IsBusy) return;
            IsBusy = true;
            UpdateRolePermissions(); // Ensure roles are current
            try
            {
                Reservations.Clear();
                List<Reservation> reservationsList;
                if (IsStaffOrAdmin)
                {
                    reservationsList = await _reservationService.GetReservationsAsync();
                    Title = "All Reservations";
                }
                else
                {
                    reservationsList = await _reservationService.GetReservationsByUserAsync(currentUser.Id);
                    Title = "My Reservations";
                }
                foreach (var res in reservationsList) Reservations.Add(res);
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading reservations: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Failed to load reservations.", "OK"); }
            finally { IsBusy = false; }
        }

        // Use the public property name for CanExecute
        [RelayCommand(CanExecute = nameof(CanAddReservation))]
        private async Task AddReservationAsync()
        {
            if (!CanAddReservation) return;
            await _navigationService.NavigateToAsync(nameof(ReservationDetailPage));
        }

        [RelayCommand] // Allow navigation, detail page handles permissions
        private async Task GoToReservationDetailAsync()
        {
            if (IsBusy || SelectedReservation == null || _authService.CurrentUser == null) return;
            await _navigationService.NavigateToAsync(nameof(ReservationDetailPage), new Dictionary<string, object> { { "ReservationId", SelectedReservation.Id } });
        }
        // TODO: Unsubscribe _authService.PropertyChanged -= AuthService_PropertyChanged;
    }
}