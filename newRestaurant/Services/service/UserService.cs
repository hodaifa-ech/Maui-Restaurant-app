using Microsoft.EntityFrameworkCore;
using Microsoft.Maui;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class UserService : IUserService
    {
        private readonly RestaurantDbContext _context;

        public UserService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAsync(int id) => await _context.Users.FindAsync(id);

        public async Task<User> GetUserByUsernameAsync(string username) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> AddUserAsync(User user, string plainPassword) // Accept plain password
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6) // Basic validation
            {
                Console.WriteLine("Error: Password is too short.");
                return false;
            }
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                Console.WriteLine($"Error: Username '{user.Username}' already exists.");
                return false;
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                Console.WriteLine($"Error: Email '{user.Email}' already exists.");
                return false;
            }

            // --- Password Hashing ---
            // ** REPLACE THIS with BCrypt in a real app! **
            // Simple placeholder (NOT SECURE!): Just prepend "HASHED_"
            // user.PasswordHash = "HASHED_" + plainPassword;
            // ** Real Implementation using BCrypt.Net-Next **
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);


            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> VerifyPasswordAsync(string username, string providedPassword)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return false; // User not found or no password stored
            }

            // --- Password Verification ---
            // ** REPLACE THIS with BCrypt in a real app! **
            // Simple placeholder check:
            // return user.PasswordHash == "HASHED_" + providedPassword;
            // ** Real Implementation using BCrypt.Net-Next **
            return BCrypt.Net.BCrypt.Verify(providedPassword, user.PasswordHash);
        }

        public Task<bool> AddUserAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}