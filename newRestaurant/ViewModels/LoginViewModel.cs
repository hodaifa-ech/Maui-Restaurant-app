// ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Services.Interfaces;
using newRestaurant.Services;
using newRestaurant.Views; // For nameof navigation
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // Required for MainThread
using newRestaurant.Models; // Required for UserRole check

namespace newRestaurant.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password;

        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private bool _hasError;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            Title = "Login";
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            if (!CanLogin()) return; // Prevent execution if CanLogin is false

            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                bool success = await _authService.LoginAsync(Username, Password);

                if (success && _authService.CurrentUser != null) // Check CurrentUser is not null
                {
                    // Successfully logged in, CurrentUser is set in AuthService
                    System.Diagnostics.Debug.WriteLine($"Login successful for {_authService.CurrentUser.Username}, Role: {_authService.CurrentUser.Role}");

                    var appShell = MauiProgram.Services.GetService<AppShell>();
                    if (appShell != null)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // Set the main page *after* successful login and potentially role check
                            Application.Current.MainPage = appShell;

                            // *** Optional: Role-based Navigation ***
                            string targetPage;
                            switch (_authService.CurrentUser.Role)
                            {
                                case UserRole.Admin:
                                case UserRole.Staff:
                                    // Navigate Staff/Admin to a management or specific staff page (e.g., Tables)
                                    targetPage = $"//{nameof(Views.TablesPage)}"; // Example: Staff/Admin go to Tables
                                    break;
                                case UserRole.Customer:
                                default:
                                    // Navigate Customer to reservations or menu page
                                    targetPage = $"//{nameof(Views.ReservationsPage)}"; // Customers go to Reservations
                                    break;
                            }

                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"Navigating to target page: {targetPage}");
                                await Shell.Current.GoToAsync(targetPage);
                                // Password = string.Empty; // Clear password field after successful navigation
                                IsBusy = false; // Set IsBusy false *after* navigation completes
                            }
                            catch (Exception navEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Navigation Error after login: {navEx}");
                                ErrorMessage = "Login successful, but failed to navigate.";
                                HasError = true;
                                IsBusy = false;
                                // Optionally log out or show error
                                // await _authService.LogoutAsync(); // Go back to login on nav error?
                                await Shell.Current.DisplayAlert("Navigation Error", "Could not navigate to the main application page.", "OK");

                            }
                        });
                    }
                    else
                    {
                        ErrorMessage = "Error loading application shell after login.";
                        HasError = true;
                        IsBusy = false;
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                    HasError = true;
                    IsBusy = false;
                    Password = string.Empty; // Clear password on failure
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                HasError = true;
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex}");
                IsBusy = false;
            }
            // finally removed - IsBusy handled in branches
        }

        [RelayCommand]
        private async Task NavigateToRegisterAsync()
        {
            if (IsBusy) return;
            await _navigationService.NavigateToAsync(nameof(RegisterPage));
        }
    }
}