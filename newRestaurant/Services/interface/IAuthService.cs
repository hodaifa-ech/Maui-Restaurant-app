// Services/Interfaces/IAuthService.cs
using newRestaurant.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface IAuthService : INotifyPropertyChanged
    {
        User CurrentUser { get; }
        bool IsLoggedIn { get; }

        Task<bool> LoginAsync(string username, string password);
        // *** UPDATED: Add UserRole parameter ***
        Task<bool> RegisterAsync(string username, string email, string password, UserRole role);
        Task LogoutAsync();
    }
}