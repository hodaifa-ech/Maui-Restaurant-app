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
    public class StatisticsService : IStatisticsService
    {
        private readonly RestaurantDbContext _context;

        public StatisticsService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var orderedCarts = await _context.Carts
                .Include(c => c.CartPlats)
                    .ThenInclude(cp => cp.Plat)
                .Where(c => c.Status == CartStatus.Ordered)
                .ToListAsync();

            return orderedCarts.Sum(cart => cart.CartPlats.Sum(cp => cp.Quantity * (cp.Plat?.Price ?? 0)));
        }

        public async Task<List<KeyValuePair<string, int>>> GetPopularDishesAsync(int topN = 5)
        {
            var popularPlats = await _context.CartPlats
                .Include(cp => cp.Cart)
                .Include(cp => cp.Plat)
                .Where(cp => cp.Cart.Status == CartStatus.Ordered && cp.Plat != null)
                .GroupBy(cp => new { cp.PlatId, cp.Plat.Name })
                .Select(g => new KeyValuePair<string, int>(
                    g.Key.Name,
                    g.Sum(cp => cp.Quantity)
                ))
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .ToListAsync();

            return popularPlats;
        }
    }
}