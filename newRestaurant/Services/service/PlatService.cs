using Microsoft.EntityFrameworkCore;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class PlatService : IPlatService
    {
        private readonly RestaurantDbContext _context;

        public PlatService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Plat>> GetPlatsAsync() => await _context.Plats.Include(p => p.Category).ToListAsync();
        public async Task<List<Plat>> GetPlatsByCategoryAsync(int categoryId) => await _context.Plats.Where(p => p.CategoryId == categoryId).Include(p => p.Category).ToListAsync();
        public async Task<Plat> GetPlatAsync(int id) => await _context.Plats.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        public async Task<bool> AddPlatAsync(Plat plat)
        {
            _context.Plats.Add(plat);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdatePlatAsync(Plat plat)
        {
            // Find the existing entity in the database
            var existingPlat = await _context.Plats.FindAsync(plat.Id);
            if (existingPlat == null)
            {
                Console.WriteLine($"Error: Plat with ID {plat.Id} not found for update.");

                return false; // Or throw not found exception
            }

            // Update properties of the tracked entity from the incoming data
            _context.Entry(existingPlat).CurrentValues.SetValues(plat);

            // Ensure CategoryId is updated if changed
            if (existingPlat.CategoryId != plat.CategoryId)
            {
                existingPlat.CategoryId = plat.CategoryId;
            }

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts if necessary
                Console.WriteLine($"Concurrency error updating plat {plat.Id}: {ex.Message}");
                // You might need to reload data and inform the user
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving plat {plat.Id}: {ex.Message}");
                return false;
            }

        }
        public async Task<bool> DeletePlatAsync(int id)
        {
            var plat = await _context.Plats.FindAsync(id);
            if (plat == null) return false;
            _context.Plats.Remove(plat);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
