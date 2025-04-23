// Services/Interfaces/ITableService.cs
using newRestaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface ITableService
    {
        Task<List<Table>> GetTablesAsync();
        Task<Table> GetTableAsync(int id);
        Task<bool> AddTableAsync(Table table);
        Task<bool> UpdateTableAsync(Table table); // <-- ADD
        Task<bool> DeleteTableAsync(int id);     // <-- ADD
    }
}