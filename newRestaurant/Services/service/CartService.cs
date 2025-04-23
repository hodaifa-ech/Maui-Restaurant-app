using Microsoft.EntityFrameworkCore;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class CartService : ICartService
    {
        private readonly RestaurantDbContext _context;

        public CartService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetActiveCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartPlats)
                    .ThenInclude(cp => cp.Plat)
                        .ThenInclude(p => p.Category)
                .Where(c => c.UserId == userId && c.Status == CartStatus.Active)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Status = CartStatus.Active, CartPlats = new List<CartPlat>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<bool> AddItemToCartAsync(int userId, int platId, int quantity = 1)
        {
            if (quantity <= 0) return false;

            var cart = await GetActiveCartAsync(userId);
            if (cart == null) return false;

            var cartItem = await _context.CartPlats
                                 .FirstOrDefaultAsync(cp => cp.CartId == cart.Id && cp.PlatId == platId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _context.CartPlats.Update(cartItem);
            }
            else
            {
                cartItem = new CartPlat { CartId = cart.Id, PlatId = platId, Quantity = quantity };
                _context.CartPlats.Add(cartItem);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveItemFromCartAsync(int cartId, int platId)
        {
            var cartItem = await _context.CartPlats.FindAsync(cartId, platId);
            if (cartItem == null) return false;

            _context.CartPlats.Remove(cartItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int cartId, int platId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveItemFromCartAsync(cartId, platId);
            }

            var cartItem = await _context.CartPlats.FindAsync(cartId, platId);
            if (cartItem == null) return false;

            cartItem.Quantity = quantity;
            _context.CartPlats.Update(cartItem);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkCartAsOrderedAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart == null || cart.Status != CartStatus.Active) return false;

            bool hasItems = await _context.CartPlats.AnyAsync(cp => cp.CartId == cartId);
            if (!hasItems) return false;

            cart.Status = CartStatus.Ordered;
            _context.Carts.Update(cart);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}