using newRestaurant.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    // Inherit INotifyPropertyChanged to signal login status changes
    public interface IAuthService : INotifyPropertyChanged
    {
        User CurrentUser { get; }
        bool IsLoggedIn { get; }

        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        Task LogoutAsync();
    }
}