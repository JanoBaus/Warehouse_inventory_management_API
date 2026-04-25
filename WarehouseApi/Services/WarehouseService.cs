using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using WarehouseApi.Models;

namespace WarehouseApi.Services
{
    public class WarehouseService : IManagementService
    {

        private static readonly SemaphoreSlim _inventoryLock = new(1, 1);
        private readonly string _filePath;
        private readonly string _metadataPath;

        public WarehouseService(IConfiguration configuration, IWebHostEnvironment environment)
            : this(ResolveDataDirectory(configuration, environment))
        {
        }

        public WarehouseService(string directory)
        {
            Directory.CreateDirectory(directory);
            _filePath = Path.Combine(directory, "inventory.json");
            _metadataPath = Path.Combine(directory, "WarehouseMetadata.json");
        }

        private static string ResolveDataDirectory(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var configuredPath = configuration["Storage:DataDirectory"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return environment.ContentRootPath;
            }

            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            return Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
        }

        private async Task<int> GetNextProductIdAsync()
        {
            WarehouseMetadata metadata;
            if (File.Exists(_metadataPath))
            {
                string json = (await File.ReadAllTextAsync(_metadataPath)).Trim();
                metadata = string.IsNullOrEmpty(json) ? new WarehouseMetadata() : JsonSerializer.Deserialize<WarehouseMetadata>(json) ?? new WarehouseMetadata();
            }
            else
            {
                metadata = new WarehouseMetadata();
            }

            return metadata.LastProductId + 1;
        }

        private async Task SaveLastProductIdAsync(int productId)
        {
            var metadata = new WarehouseMetadata { LastProductId = productId };
            string updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_metadataPath, updatedJson);
        }

        private async Task<List<Product>> GetAllInternalAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Product>();
            }

            string json = (await File.ReadAllTextAsync(_filePath)).Trim();

            if (string.IsNullOrEmpty(json))
            {
                return new List<Product>();
            }

            return JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
        }

        public async Task<List<Product>> GetAllAsync()
        {
            await _inventoryLock.WaitAsync();
            try
            {
                return await GetAllInternalAsync();
            }
            finally
            {
                _inventoryLock.Release();
            }
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            await _inventoryLock.WaitAsync();
            try
            {
                var products = await GetAllInternalAsync();
                return products.FirstOrDefault(p => p.Id == id);
            }
            finally
            {
                _inventoryLock.Release();
            }
        }

        private async Task SaveToFileAsync(List<Product> products)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(products, options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task AddProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ValidateProduct(product);

            await _inventoryLock.WaitAsync();
            try
            {
                var list = await GetAllInternalAsync();
                product.Id = await GetNextProductIdAsync();
                list.Add(product);
                await SaveToFileAsync(list);
                await SaveLastProductIdAsync(product.Id);
            }
            finally
            {
                _inventoryLock.Release();
            }
        }

        public async Task UpdateProductAsync(int id, Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ValidateProduct(product);

            await _inventoryLock.WaitAsync();
            try
            {
                var list = await GetAllInternalAsync();
                var existingProduct = list.FirstOrDefault(p => p.Id == id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;

                await SaveToFileAsync(list);
            }
            finally
            {
                _inventoryLock.Release();
            }
        }

        public void ValidateProduct(Product product)
        {
            if (product.Price < 0)
            {
                throw new ArgumentException("Product price cannot be negative.");
            }

            if (product.StockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative.");
            }
        }

    }
}