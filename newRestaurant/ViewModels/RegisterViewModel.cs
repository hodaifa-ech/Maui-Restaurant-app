// ViewModels/RegisterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models; // Needed for UserRole
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System.Collections.Generic; // Needed for List
using System.Linq; // Needed for Enum manipulation
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

        // *** ADDED Role Properties ***
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))] // Add validation if needed
        private UserRole _selectedRole = UserRole.Customer; // Default to Customer

        public List<UserRole> AvailableRoles { get; private set; }
        // *** END ADDED ***

        public RegisterViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            Title = "Register";

            // *** ADDED: Populate Available Roles (Exclude Admin for self-registration) ***
            AvailableRoles = Enum.GetValues(typeof(UserRole))
                                 .Cast<UserRole>()
                                 .Where(role => role != UserRole.Admin) // Prevent self-registering as Admin
                                 .ToList();
            // Set a default if needed, although the ObservableProperty handles it
            // SelectedRole = AvailableRoles.FirstOrDefault();
        }

        private bool CanRegister() =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Email) && // Add email format validation later
            !string.IsNullOrWhiteSpace(Password) && Password.Length >= 6 &&
            Password == ConfirmPassword &&
            // Can add check for SelectedRole if needed (e.g., ensure one is selected)
            // Enum.IsDefined(typeof(UserRole), SelectedRole) && // Check if valid enum (good practice)
            !IsBusy;


        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    HasError = true;
                    IsBusy = false; // Ensure IsBusy is reset
                    return;
                }

                // *** UPDATED: Pass SelectedRole to RegisterAsync ***
                bool success = await _authService.RegisterAsync(Username, Email, Password, SelectedRole);

                if (success)
                {
                    // Use MainThread for UI interaction after async operation
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.DisplayAlert("Success", "Registration successful! Please log in.", "OK");
                    });
                    await GoBackAsync(); // Go back to login page
                }
                else
                {
                    ErrorMessage = "Registration failed. Username or email might already exist, or an error occurred.";
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