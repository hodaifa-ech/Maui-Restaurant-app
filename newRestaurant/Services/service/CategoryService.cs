// Services/CategoryService.cs
using Microsoft.EntityFrameworkCore;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly RestaurantDbContext _context;

        public CategoryService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetCategoriesAsync() => await _context.Categories.ToListAsync();
        public async Task<Category> GetCategoryAsync(int id) => await _context.Categories.FindAsync(id);
        public async Task<bool> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null) return false;
            _context.Entry(existing).CurrentValues.SetValues(category);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            // Optional: Add check for existing Plats before allowing delete
            if (await _context.Plats.AnyAsync(p => p.CategoryId == id))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot delete category {id}, it has associated dishes.");
                await Shell.Current.DisplayAlert("Delete Failed", "Cannot delete category with associated dishes.", "OK");
                return false;
            }

            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}