using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel; // Can use ObservableObject

namespace newRestaurant.Services
{
    // Use ObservableObject for easy INotifyPropertyChanged implementation
    public partial class AuthService : ObservableObject, IAuthService
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService; // To navigate on logout

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoggedIn))] // Notify IsLoggedIn changes when CurrentUser changes
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
                // Fetch the full user object on successful login
                _currentUser = await _userService.GetUserByUsernameAsync(username);
                System.Diagnostics.Debug.WriteLine($"User '{_currentUser?.Username}' logged in.");
                return true;
            }
            else
            {
                _currentUser = null; // Ensure user is logged out on failure
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;
            // Add more validation as needed (email format etc.)

            var newUser = new User
            {
                Username = username.Trim(),
                Email = email.Trim().ToLower(),
                // Role assignment logic could go here
                Role = UserRole.Customer
            };

            // Use the UserService method that handles hashing
            bool success = await _userService.AddUserAsync(newUser, password);
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"User '{username}' registered successfully.");
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
            CurrentUser = null; // Clear the user state
            System.Diagnostics.Debug.WriteLine($"User '{username}' logged out.");

            // Navigate back to the login page (absolute route to clear stack)
            // Ensure LoginPage is registered with "//LoginPage" or similar absolute route
            await _navigationService.NavigateToAsync($"//{nameof(Views.LoginPage)}");
            
        }
    }
}