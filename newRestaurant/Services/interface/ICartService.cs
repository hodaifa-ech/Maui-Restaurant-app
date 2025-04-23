using newRestaurant.Models;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart> GetActiveCartAsync(int userId);
        Task<bool> AddItemToCartAsync(int userId, int platId, int quantity = 1);
        Task<bool> RemoveItemFromCartAsync(int cartId, int platId);
        Task<bool> UpdateCartItemQuantityAsync(int cartId, int platId, int quantity);
        Task<bool> MarkCartAsOrderedAsync(int cartId);
    }
}