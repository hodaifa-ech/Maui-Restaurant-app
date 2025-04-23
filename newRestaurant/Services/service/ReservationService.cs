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
                          .Include(r => r.User)
                          .Include(r => r.Table)
                          .OrderByDescending(r => r.TimeStart)
                          .ToListAsync();

        public async Task<List<Reservation>> GetReservationsByUserAsync(int userId) =>
            await _context.Reservations
                          .Where(r => r.UserId == userId)
                          .Include(r => r.User)
                          .Include(r => r.Table)
                          .ToListAsync();

        public async Task<Reservation> GetReservationAsync(int id) =>
            await _context.Reservations
                          .Include(r => r.User)
                          .Include(r => r.Table)
                          .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<bool> AddReservationAsync(Reservation reservation)
        {
            bool overlaps = await _context.Reservations
                .AnyAsync(r => r.TableId == reservation.TableId &&
                               r.Status != ReservationStatus.Cancelled &&
                               r.TimeStart < reservation.TimeEnd &&
                               r.TimeEnd > reservation.TimeStart);
            if (overlaps)
            {
                Console.WriteLine($"Error: Reservation overlap detected for TableId {reservation.TableId}");
                return false;
            }
            _context.Reservations.Add(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        // Services/ReservationService.cs
        public async Task<bool> UpdateReservationAsync(Reservation reservation) // reservation is the reservationToSave object
        {
            // Optional: Add overlap check here if needed
            // bool overlaps = await _context.Reservations ... (excluding the current reservation ID)

            var existingReservation = await _context.Reservations.FindAsync(reservation.Id);
            if (existingReservation == null)
            {
                Console.WriteLine($"Error: Reservation with ID {reservation.Id} not found for update.");
                return false;
            }

            // Copy scalar values
            _context.Entry(existingReservation).CurrentValues.SetValues(reservation);

            // Explicitly update FKs if necessary (though SetValues often covers it)
            if (existingReservation.TableId != reservation.TableId)
            {
                existingReservation.TableId = reservation.TableId;
            }
            if (existingReservation.UserId != reservation.UserId) // Should not change usually, but possible
            {
                existingReservation.UserId = reservation.UserId;
            }

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating reservation {reservation.Id}: {ex.Message}");
                return false;
            }
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
