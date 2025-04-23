// Services/AuthService.cs
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using newRestaurant.Views; // For logout navigation

namespace newRestaurant.Services
{
    public partial class AuthService : ObservableObject, IAuthService
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
        private User _currentUser;

        public bool IsLoggedIn => _currentUser != null;

        public AuthService(IUserService userService, INavigationService navigationService)
        {
            _userService = userService;
            _navigationService = navigationService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            bool isValid = await _userService.VerifyPasswordAsync(username, password);

            if (isValid)
            {
                // Fetch the full user object (including role) on successful login
                CurrentUser = await _userService.GetUserByUsernameAsync(username); // Use property setter
                if (CurrentUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User '{CurrentUser.Username}' logged in with Role: {CurrentUser.Role}.");
                    // Trigger PropertyChanged for CurrentUser and IsLoggedIn automatically via [ObservableProperty]
                    return true;
                }
                else
                {
                    // Should not happen if VerifyPasswordAsync passed, but good to handle
                    System.Diagnostics.Debug.WriteLine($"Login valid but failed to fetch user '{username}'.");
                    CurrentUser = null; // Ensure state is clean
                    return false;
                }
            }
            else
            {
                CurrentUser = null; // Ensure user is logged out on failure
                return false;
            }
        }

        // *** UPDATED: Implement new RegisterAsync signature ***
        public async Task<bool> RegisterAsync(string username, string email, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            // Prevent registering as Admin directly through this method if desired
            if (role == UserRole.Admin)
            {
                System.Diagnostics.Debug.WriteLine("Attempted to register as Admin directly. Denied.");
                return false; // Or throw an exception
            }

            var newUser = new User
            {
                Username = username.Trim(),
                Email = email.Trim().ToLower(),
                Role = role // Assign the selected role
            };

            // Use the UserService method that handles hashing
            bool success = await _userService.AddUserAsync(newUser, password);
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"User '{username}' registered successfully with Role: {role}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed for user '{username}'.");
            }
            return success;
        }

        public async Task LogoutAsync()
        {
            var username = CurrentUser?.Username; // Get username before logging out
            CurrentUser = null; // Clear the user state (triggers PropertyChanged)
            System.Diagnostics.Debug.WriteLine($"User '{username}' logged out.");

            // Navigate back to the login page
            await _navigationService.NavigateToAsync($"//{nameof(LoginPage)}");
        }

        // No changes needed to OnPropertyChanged if using ObservableObject
    }
}