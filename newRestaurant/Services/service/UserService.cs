// Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Diagnostics; // For Debug
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

        public async Task<bool> AddUserAsync(User user, string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6) { Debug.WriteLine("Error: Password too short."); return false; }
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == user.Username.ToLower())) { Debug.WriteLine($"Error: Username '{user.Username}' exists."); return false; }
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower())) { Debug.WriteLine($"Error: Email '{user.Email}' exists."); return false; }

            try { user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword); }
            catch (Exception ex) { Debug.WriteLine($"Error hashing password: {ex.Message}"); return false; }

            _context.Users.Add(user);
            try { return await _context.SaveChangesAsync() > 0; }
            catch (Exception ex) { Debug.WriteLine($"Error saving new user: {ex.Message}"); return false; }
        }

        public async Task<bool> VerifyPasswordAsync(string username, string providedPassword)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return false;
            try { return BCrypt.Net.BCrypt.Verify(providedPassword, user.PasswordHash); }
            catch { return false; } // Handle potential bcrypt errors
        }

        // Not used - keep AddUserAsync with password
        // public Task<bool> AddUserAsync(User user) => throw new NotImplementedException();


        // *** ADDED ***
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null || user.Id <= 0) return false;

            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                Debug.WriteLine($"Error: User with ID {user.Id} not found for update.");
                return false;
            }

            // Check for uniqueness constraints ONLY IF the value changed
            if (existingUser.Username != user.Username && await _context.Users.AnyAsync(u => u.Username.ToLower() == user.Username.ToLower() && u.Id != user.Id))
            {
                Debug.WriteLine($"Error: Updated username '{user.Username}' already exists.");
                return false;
            }
            if (existingUser.Email != user.Email && await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Id != user.Id))
            {
                Debug.WriteLine($"Error: Updated email '{user.Email}' already exists.");
                return false;
            }

            // Update only changeable fields (Username, Email). Don't update Role or PasswordHash here.
            existingUser.Username = user.Username.Trim();
            existingUser.Email = user.Email.Trim().ToLower();
            // existingUser.Role = user.Role; // Decide if Role should be editable here

            _context.Users.Update(existingUser); // Or _context.Entry(existingUser).State = EntityState.Modified;
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException ex) { Debug.WriteLine($"Concurrency error updating user {user.Id}: {ex.Message}"); return false; }
            catch (Exception ex) { Debug.WriteLine($"Error updating user {user.Id}: {ex.Message}"); return false; }
        }

        // *** ADDED ***
        public async Task<bool> DoesEmailExistForAnotherUserAsync(string email, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Check if any OTHER user (Id != currentUserId) has this email
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != currentUserId);
        }
    }
}