using newRestaurant.Models;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> AddUserAsync(User user);
        Task<bool> AddUserAsync(User user, string plainPassword); // Accept plain password

        Task<bool> VerifyPasswordAsync(string username, string providedPassword);
    }
}
