// Services/Interfaces/IPlatService.cs
using newRestaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface IPlatService
    {
        Task<List<Plat>> GetPlatsAsync();
        Task<List<Plat>> GetPlatsByCategoryAsync(int categoryId);
        Task<Plat> GetPlatAsync(int id);
        Task<bool> AddPlatAsync(Plat plat);
        Task<bool> UpdatePlatAsync(Plat plat);
        Task<bool> DeletePlatAsync(int id);
    }
}