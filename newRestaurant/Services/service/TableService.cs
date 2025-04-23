// Services/TableService.cs
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
    public class TableService : ITableService
    {
        private readonly RestaurantDbContext _context;

        public TableService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Table>> GetTablesAsync() =>
            await _context.Tables.OrderBy(t => t.TableNumber).ToListAsync();

        public async Task<Table> GetTableAsync(int id) =>
            await _context.Tables.FindAsync(id);

        public async Task<bool> AddTableAsync(Table table)
        {
            if (await _context.Tables.AnyAsync(t => t.TableNumber.ToLower() == table.TableNumber.ToLower()))
            {
                Console.WriteLine($"Error: Table number '{table.TableNumber}' already exists.");
                return false;
            }
            _context.Tables.Add(table);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTableAsync(Table table)
        {
            if (await _context.Tables.AnyAsync(t => t.TableNumber.ToLower() == table.TableNumber.ToLower() && t.Id != table.Id))
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

            _context.Entry(existingTable).CurrentValues.SetValues(table);

            try { return await _context.SaveChangesAsync() > 0; }
            catch (Exception ex) { Console.WriteLine($"Error updating table {table.Id}: {ex.Message}"); return false; }
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await _context.Tables
                                    .Include(t => t.Reservations)
                                    .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return false;

            // *** Check for active/future reservations ***
            if (table.Reservations.Any(r => r.TimeEnd > DateTime.Now && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Completed))
            {
                Console.WriteLine($"Error: Cannot delete table {table.Id} ('{table.TableNumber}') as it has active or upcoming reservations.");
                await Shell.Current.DisplayAlert("Delete Failed", "Cannot delete table with active or upcoming reservations.", "OK");
                return false;
            }

            _context.Tables.Remove(table);
            try { return await _context.SaveChangesAsync() > 0; }
            catch (DbUpdateException ex) { Console.WriteLine($"Error deleting table {id} (FK constraint?): {ex.InnerException?.Message ?? ex.Message}"); await Shell.Current.DisplayAlert("Delete Failed", "Could not delete table. It might still be referenced.", "OK"); return false; }
            catch (Exception ex) { Console.WriteLine($"Error deleting table {id}: {ex.Message}"); return false; }
        }
    }
}