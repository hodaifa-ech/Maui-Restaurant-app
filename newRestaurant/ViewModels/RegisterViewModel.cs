// ViewModels/RegisterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _username;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _email;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _password;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string _confirmPassword;

        [ObservableProperty] private string _errorMessage;
        [ObservableProperty] private bool _hasError;

        public RegisterViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            Title = "Register";
        }

        private bool CanRegister() =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Email) && // Add email validation later
            !string.IsNullOrWhiteSpace(Password) && Password.Length >= 6 &&
            Password == ConfirmPassword &&
            !IsBusy;


        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                // Basic validation already in CanExecute, but can add more here (e.g., email format)
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    HasError = true;
                    return; // Exit early
                }

                bool success = await _authService.RegisterAsync(Username, Email, Password);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Registration successful! Please log in.", "OK");
                    await GoBackAsync(); // Go back to login page
                }
                else
                {
                    // More specific error message could be provided by AuthService/UserService
                    ErrorMessage = "Registration failed. Username or email might already exist.";
                    HasError = true;
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                HasError = true;
                System.Diagnostics.Debug.WriteLine($"Registration Error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            if (IsBusy) return;
            await _navigationService.GoBackAsync();
        }
    }
}