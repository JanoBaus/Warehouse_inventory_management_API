using System.Text.Json;
using WarehouseApi.Models;

public class WarehouseService : IManagementService
{
    private readonly string _filePath;
    private readonly string _metadataPath;

    public WarehouseService() : this(Directory.GetCurrentDirectory()) { }

    public WarehouseService(string directory)
    {
        _filePath = Path.Combine(directory, "Inventory.json");
        _metadataPath = Path.Combine(directory, "WarehouseMetadata.json");
    }

    private int GetNextProductId()
    {
        WarehouseMetadata metadata;
        if (File.Exists(_metadataPath))
        {
            string json = File.ReadAllText(_metadataPath).Trim();
            metadata = string.IsNullOrEmpty(json) ? new WarehouseMetadata() : JsonSerializer.Deserialize<WarehouseMetadata>(json) ?? new WarehouseMetadata();
        }
        else
        {
            metadata = new WarehouseMetadata();
        }

        metadata.LastProductId++;
        string updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_metadataPath, updatedJson);

        return metadata.LastProductId;
    }

    public List<Product> GetAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Product>();
        }

        string json = File.ReadAllText(_filePath).Trim();

        if (string.IsNullOrEmpty(json))
        {
            return new List<Product>();
        }

        return JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
    }

    public Product? GetById(int id)
    {
        var products = GetAll();
        return products.FirstOrDefault(p => p.Id == id);
    }

    private void SaveToFile(List<Product> products)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(products, options);
        File.WriteAllText(_filePath, json);
    }

    public void AddProduct(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }
        if (product.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.");
        }
        if (product.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        var list = GetAll();
        product.Id = GetNextProductId();
        list.Add(product);
        SaveToFile(list);
    }

    public void UpdateProduct(int id, Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }
        if (product.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.");
        }
        if (product.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        var list = GetAll();
        var existingProduct = list.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.StockQuantity = product.StockQuantity;

        SaveToFile(list);
    }
}