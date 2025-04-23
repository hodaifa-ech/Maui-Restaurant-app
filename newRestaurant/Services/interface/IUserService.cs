// Services/Interfaces/IUserService.cs
using newRestaurant.Models;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> AddUserAsync(User user, string plainPassword); // Keep this for registration
        Task<bool> VerifyPasswordAsync(string username, string providedPassword);

        // *** ADDED ***
        // Update user details (username, email - NOT password hash)
        Task<bool> UpdateUserAsync(User user);
        // Check if email exists for a DIFFERENT user (for update validation)
        Task<bool> DoesEmailExistForAnotherUserAsync(string email, int currentUserId);

        // Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword); // Optional: Separate password change
    }
}