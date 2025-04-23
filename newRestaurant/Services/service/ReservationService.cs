// Services/ReservationService.cs
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
    public class ReservationService : IReservationService
    {
        private readonly RestaurantDbContext _context;

        public ReservationService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reservation>> GetReservationsAsync() =>
            await _context.Reservations
                          .Include(r => r.User) // Include User details
                          .Include(r => r.Table) // Include Table details
                          .OrderByDescending(r => r.TimeStart)
                          .ToListAsync();

        public async Task<List<Reservation>> GetReservationsByUserAsync(int userId) =>
            await _context.Reservations
                          .Where(r => r.UserId == userId)
                          .Include(r => r.User)
                          .Include(r => r.Table)
                          .OrderByDescending(r => r.TimeStart) // Order user's reservations
                          .ToListAsync();

        public async Task<Reservation> GetReservationAsync(int id) =>
            await _context.Reservations
                          .Include(r => r.User)
                          .Include(r => r.Table)
                          .FirstOrDefaultAsync(r => r.Id == id);

        // Basic overlap check before adding
        private async Task<bool> CheckForOverlapAsync(int tableId, DateTime start, DateTime end, int? excludeReservationId = null)
        {
            // Check for any reservation on the same table that is NOT cancelled/completed
            // AND overlaps with the proposed time range.
            // If excludeReservationId is provided (when updating), exclude that specific reservation from the check.
            return await _context.Reservations
                .AnyAsync(r => r.TableId == tableId &&
                               r.Status != ReservationStatus.Cancelled &&
                               r.Status != ReservationStatus.Completed &&
                               (excludeReservationId == null || r.Id != excludeReservationId) && // Exclude self if updating
                               r.TimeStart < end && // Existing reservation starts before the new one ends
                               r.TimeEnd > start); // Existing reservation ends after the new one starts
        }

        public async Task<bool> AddReservationAsync(Reservation reservation)
        {
            // Validate times
            if (reservation.TimeEnd <= reservation.TimeStart)
            {
                Console.WriteLine("Error: Reservation end time must be after start time.");
                return false;
            }

            // Check for overlaps before adding
            if (await CheckForOverlapAsync(reservation.TableId, reservation.TimeStart, reservation.TimeEnd))
            {
                Console.WriteLine($"Error: Reservation overlap detected for TableId {reservation.TableId} at the requested time.");
                // Consider providing this feedback to the user via ViewModel/Exception
                return false;
            }

            _context.Reservations.Add(reservation);
            try { return await _context.SaveChangesAsync() > 0; }
            catch (Exception ex) { Console.WriteLine($"Error adding reservation: {ex.Message}"); return false; }
        }

        public async Task<bool> UpdateReservationAsync(Reservation reservation)
        {
            // Validate times
            if (reservation.TimeEnd <= reservation.TimeStart)
            {
                Console.WriteLine("Error: Reservation end time must be after start time.");
                return false;
            }

            var existingReservation = await _context.Reservations.FindAsync(reservation.Id);
            if (existingReservation == null)
            {
                Console.WriteLine($"Error: Reservation with ID {reservation.Id} not found for update.");
                return false;
            }

            // Check for overlaps before updating, excluding the current reservation ID
            if (await CheckForOverlapAsync(reservation.TableId, reservation.TimeStart, reservation.TimeEnd, reservation.Id))
            {
                Console.WriteLine($"Error: Updated reservation time overlaps with another reservation for TableId {reservation.TableId}.");
                return false;
            }


            // Copy scalar values from the incoming object to the tracked entity
            _context.Entry(existingReservation).CurrentValues.SetValues(reservation);

            // Ensure Foreign Keys are set correctly if they can change (though UserId usually shouldn't)
            if (existingReservation.TableId != reservation.TableId) existingReservation.TableId = reservation.TableId;
            if (existingReservation.UserId != reservation.UserId) existingReservation.UserId = reservation.UserId; // Should be rare


            try { return await _context.SaveChangesAsync() > 0; }
            catch (Exception ex) { Console.WriteLine($"Error updating reservation {reservation.Id}: {ex.Message}"); return false; }
        }

        public async Task<bool> DeleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;
            _context.Reservations.Remove(reservation);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}