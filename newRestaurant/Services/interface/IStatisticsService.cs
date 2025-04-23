// Services/Interfaces/IStatisticsService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<decimal> GetTotalRevenueAsync();
        Task<List<KeyValuePair<string, int>>> GetPopularDishesAsync(int topN = 5);
    }
}