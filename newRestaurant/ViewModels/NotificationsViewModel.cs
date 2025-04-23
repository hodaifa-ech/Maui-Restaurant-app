// ViewModels/NotificationsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls; // For Shell
using System.Linq; // Needed for Any()

namespace newRestaurant.ViewModels
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // !!!!! ENSURE THIS 'partial' KEYWORD IS REALLY PRESENT !!!!!
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public partial class NotificationsViewModel : BaseViewModel
    {
        private readonly INotificationService _notificationService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Notification> _notifications = new();

        [ObservableProperty]
        private bool _showReadNotifications = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNotifications))]
        private int _notificationCount;

        // Corrected property name used in XAML IsEnabled binding
        public bool HasNotifications => _notificationCount > 0;


        public NotificationsViewModel(INotificationService notificationService, IAuthService authService)
        {
            _notificationService = notificationService;
            _authService = authService;
            Title = "Notifications";

            _authService.PropertyChanged += AuthService_PropertyChanged;
        }

        private async void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser))
            {
                // Use the generated command property name
                object value =  LoadNotificationsCommand.ExecuteAsync(null);
            }
        }

        // The [RelayCommand] attribute generates 'LoadNotificationsCommand'
        [RelayCommand]
        private async Task LoadNotificationsAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) { _notifications.Clear(); _notificationCount = 0; OnPropertyChanged(nameof(HasNotifications)); return; } // Update HasNotifications too
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var notificationList = await _notificationService.GetNotificationsAsync(currentUser.Id, _showReadNotifications);
                _notifications.Clear(); // Use property setter for ObservableCollection if needed
                foreach (var notification in notificationList) _notifications.Add(notification);
                _notificationCount = _notifications.Count; // Use property setter
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading notifications: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "Failed to load notifications.", "OK"); }
            finally { IsBusy = false; }
        }

        // The [ObservableProperty] attribute above generates the needed partial method signature.
        // This is the implementation part.
        partial void OnShowReadNotificationsChanged(bool value)
        {
            // Use the generated command property name
            LoadNotificationsCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task MarkAsReadAsync(Notification notification)
        {
            if (notification == null || notification.IsRead || IsBusy) return;
            IsBusy = true;
            try
            {
                bool success = await _notificationService.MarkAsReadAsync(notification.Id);
                if (success)
                {
                    if (!_showReadNotifications) { _notifications.Remove(notification); _notificationCount = _notifications.Count; }
                    else
                    {
                        notification.IsRead = true;
                        // For simple UI updates where IsRead affects opacity/style directly in XAML,
                        // no further action might be needed if the binding updates.
                        // If Notification needed INotifyPropertyChanged itself:
                        // var itemInList = Notifications.FirstOrDefault(n=> n.Id == notification.Id);
                        // itemInList?.NotifyIsReadChanged(); // Hypothetical method if Notification was observable
                        // Force refresh the specific item if needed, though less common
                        // OnPropertyChanged(nameof(Notifications)); // Less efficient, rebinds whole list
                    }
                }
                else { await Shell.Current.DisplayAlert("Error", "Could not mark notification as read.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error marking notification as read: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "An error occurred.", "OK"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task MarkAllAsReadAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null || IsBusy || !_notifications.Any(n => !n.IsRead)) return; // Check using property
            bool confirm = await Shell.Current.DisplayAlert("Confirm", "Mark all notifications as read?", "Yes", "No");
            if (!confirm) return;
            IsBusy = true;
            try
            {
                bool success = await _notificationService.MarkAllAsReadAsync(currentUser.Id);
                if (success)
                {
                    // Reload is essential here as multiple items changed
                    await LoadNotificationsAsync();
                }
                else { await Shell.Current.DisplayAlert("Error", "Could not mark all notifications as read.", "OK"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Error marking all as read: {ex.Message}"); await Shell.Current.DisplayAlert("Error", "An error occurred.", "OK"); }
            finally { IsBusy = false; }
        }
    }
}