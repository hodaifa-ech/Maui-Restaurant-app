// ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Services.Interfaces;
using newRestaurant.Services;

using newRestaurant.Views; // For nameof navigation
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // Required for MainThread

namespace newRestaurant.ViewModels
{
    public partial class LoginViewModel : BaseViewModel // Assuming BaseViewModel exists
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService; // *** RE-ADD this dependency ***

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password;

        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private bool _hasError;

        // *** Update Constructor to include INavigationService ***
        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService; // *** Assign it ***
            Title = "Login";
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                bool success = await _authService.LoginAsync(Username, Password);

                if (success)
                {
                    var appShell = MauiProgram.Services.GetService<AppShell>(); // Assumes static accessor
                    if (appShell != null)
                    {
                        Application.Current.MainPage = appShell;

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                await Shell.Current.GoToAsync($"//{nameof(Views.ReservationsPage)}");
                            }
                            catch (Exception navEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Navigation Error after login: {navEx}");
                                await Shell.Current.DisplayAlert("Navigation Error", "Could not navigate to the reservations page.", "OK");
                            }
                        });
                    }
                    else
                    {
                        ErrorMessage = "Error loading application shell.";
                        HasError = true;
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                    HasError = true;
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                HasError = true;
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex}");
            }
            finally
            {
                // Setting IsBusy in branches is better than here
                // IsBusy = false; // Remove from finally if set in all paths
                if (HasError) IsBusy = false; // Ensure IsBusy is false if login fails
            }
        }

        // *** RE-ADD or UNCOMMENT this method and its attribute ***
        [RelayCommand]
        private async Task NavigateToRegisterAsync()
        {
            if (IsBusy) return;
            // Now uses the injected INavigationService again
            await _navigationService.NavigateToAsync(nameof(RegisterPage));
        }
    }
}