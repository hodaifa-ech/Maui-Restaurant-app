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
            _context.Entry(category).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}