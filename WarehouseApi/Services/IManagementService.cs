using WarehouseApi.Models;

namespace WarehouseApi.Services
{
    public interface IManagementService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(int id, Product product);
    }
}