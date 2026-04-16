using System.Text.Json;
using WarehouseApi.Models;

public class WarehouseService: IManagementService
{
    private const string FilePath = "Inventory.json";
    private const string MetadataPath = "WarehouseMetadata.json";

    private int GetNextProductId()
    {
        WarehouseMetadata metadata;
        if (File.Exists(MetadataPath))
        {
            string json = File.ReadAllText(MetadataPath).Trim();
            metadata = string.IsNullOrEmpty(json) ? new WarehouseMetadata() : JsonSerializer.Deserialize<WarehouseMetadata>(json) ?? new WarehouseMetadata();
        }
        else
        {
            metadata = new WarehouseMetadata();
        }

        metadata.LastProductId++;
        string updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(MetadataPath, updatedJson);

        return metadata.LastProductId;
    }

    public List<Product> GetAll()
    {
        if(!File.Exists(FilePath))
        {
            return new List<Product>();
        }

        string json = File.ReadAllText(FilePath).Trim();
      
        if(string.IsNullOrEmpty(json))
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
        File.WriteAllText(FilePath, json);
    }

    public void AddProduct(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }
        if(product.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.");
        }
        if(product.Price < 0)
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
        if(product.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.");
        }
        if(product.Price < 0)
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