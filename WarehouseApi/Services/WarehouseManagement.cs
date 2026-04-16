using System.Text.Json;

public class WarehouseManagement
{
    private const string FilePath = "Inventory.json";

    public void ClearAll()
    {
        SaveToFile(new List<Product>());
    }

    public List<Product> GetAll()
    {
        if(!File.Exists(FilePath))
        {
            return new List<Product>();
        }

        string json = File.ReadAllText(FilePath);
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
        if(product.Quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.");
        }
        if(product.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative.");
        }

        var list = GetAll();
        product.Id = list.Select(p => p.Id).DefaultIfEmpty(0).Max() + 1;
        list.Add(product);
        SaveToFile(list);
    }

    public void UpdateProduct(int id, Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }
        if(product.Quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.");
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
        existingProduct.Quantity = product.Quantity;

        SaveToFile(list);
    }

    public void DeleteProduct(int id)
    {
        var list = GetAll();
        var product = list.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        list.Remove(product);
        SaveToFile(list);
    }
    

}