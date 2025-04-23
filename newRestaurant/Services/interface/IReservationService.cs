using newRestaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<Reservation>> GetReservationsAsync();
        Task<List<Reservation>> GetReservationsByUserAsync(int userId);
        Task<Reservation> GetReservationAsync(int id);
        Task<bool> AddReservationAsync(Reservation reservation);
        Task<bool> UpdateReservationAsync(Reservation reservation);
        Task<bool> DeleteReservationAsync(int id);
    }
}