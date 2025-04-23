// Services/TableService.cs
using Microsoft.EntityFrameworkCore;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq; // Needed for AnyAsync
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class TableService : ITableService
    {
        private readonly RestaurantDbContext _context;

        public TableService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Table>> GetTablesAsync() =>
            await _context.Tables.OrderBy(t => t.TableNumber).ToListAsync(); // Optional ordering

        public async Task<Table> GetTableAsync(int id) =>
            await _context.Tables.FindAsync(id);

        public async Task<bool> AddTableAsync(Table table)
        {
            // Optional: Add validation for duplicate TableNumber
            if (await _context.Tables.AnyAsync(t => t.TableNumber == table.TableNumber))
            {
                Console.WriteLine($"Error: Table number '{table.TableNumber}' already exists.");
                // Consider throwing a specific exception or returning a more informative result
                return false;
            }
            _context.Tables.Add(table);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- ADDED Update ---
        public async Task<bool> UpdateTableAsync(Table table)
        {
            // Optional: Check for duplicate TableNumber if it can be changed
            if (await _context.Tables.AnyAsync(t => t.TableNumber == table.TableNumber && t.Id != table.Id))
            {
                Console.WriteLine($"Error: Table number '{table.TableNumber}' already exists for another table.");
                return false;
            }

            var existingTable = await _context.Tables.FindAsync(table.Id);
            if (existingTable == null)
            {
                Console.WriteLine($"Error: Table with ID {table.Id} not found for update.");
                return false;
            }

            // Update scalar properties
            _context.Entry(existingTable).CurrentValues.SetValues(table);

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating table {table.Id}: {ex.Message}");
                return false;
            }
        }

        // --- ADDED Delete ---
        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await _context.Tables
                                    .Include(t => t.Reservations) // Include reservations to check if any exist
                                    .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return false; // Not found
            }

            // **Important Check:** Prevent deleting a table if it has active/future reservations
            if (table.Reservations.Any(r => r.TimeEnd > DateTime.Now && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Completed))
            {
                Console.WriteLine($"Error: Cannot delete table {table.Id} ('{table.TableNumber}') as it has active or upcoming reservations.");
                // Optionally display this message to the user via the ViewModel
                await Shell.Current.DisplayAlert("Delete Failed", "Cannot delete table with active or upcoming reservations.", "OK");
                return false;
            }
            // Add more checks if needed (e.g., relationship to active orders?)

            _context.Tables.Remove(table);
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex) // Catch potential FK constraint issues if checks above are insufficient
            {
                Console.WriteLine($"Error deleting table {id} (potential FK violation): {ex.InnerException?.Message ?? ex.Message}");
                await Shell.Current.DisplayAlert("Delete Failed", "Could not delete table. It might still be referenced.", "OK");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting table {id}: {ex.Message}");
                return false;
            }
        }
    }
}