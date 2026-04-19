using System.Text.Json;
using Xunit;

public class WarehouseService_GetAll_Tests : WarehouseServiceTestBase
{
    [Fact]
    public void Returns_EmptyList_When_FileDoesNotExist()
    {
        var result = CreateService().GetAll();
        Assert.Empty(result);
    }

    [Fact]
    public void Returns_EmptyList_When_FileIsEmpty()
    {
        File.WriteAllText(Path.Combine(TestDir, "Inventory.json"), "");
        var result = CreateService().GetAll();
        Assert.Empty(result);
    }

    [Fact]
    public void Returns_AllProducts_When_FileHasData()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Widget", Price = 9.99m, StockQuantity = 10 },
            new Product { Id = 2, Name = "Gadget", Price = 4.99m, StockQuantity = 5  },
        });

        var result = CreateService().GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Returns_CorrectProductData_When_FileHasData()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Widget", Price = 9.99m, StockQuantity = 10 }
        });

        var product = CreateService().GetAll()[0];

        Assert.Equal(1,        product.Id);
        Assert.Equal("Widget", product.Name);
        Assert.Equal(9.99m,    product.Price);
        Assert.Equal(10,       product.StockQuantity);
    }
}

public class WarehouseService_GetById_Tests : WarehouseServiceTestBase
{
    [Fact]
    public void Returns_CorrectProduct_When_IdExists()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Alpha", Price = 1m, StockQuantity = 1 },
            new Product { Id = 2, Name = "Beta",  Price = 2m, StockQuantity = 2 },
        });

        var result = CreateService().GetById(2);

        Assert.NotNull(result);
        Assert.Equal(2,      result!.Id);
        Assert.Equal("Beta", result.Name);
    }

    [Fact]
    public void Returns_Null_When_IdDoesNotExist()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Alpha", Price = 1m, StockQuantity = 1 }
        });

        Assert.Null(CreateService().GetById(999));
    }

    [Fact]
    public void Returns_Null_When_InventoryIsEmpty()
    {
        Assert.Null(CreateService().GetById(1));
    }
}

public class WarehouseService_AddProduct_Tests : WarehouseServiceTestBase
{
    [Fact]
    public void Assigns_Id_Of_1_When_NoProductsExist()
    {
        var product = new Product { Name = "New", Price = 1m, StockQuantity = 1 };
        CreateService().AddProduct(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public void Assigns_IncrementingIds_For_MultipleProducts()
    {
        var svc = CreateService();
        var p1 = new Product { Name = "First",  Price = 1m, StockQuantity = 1 };
        var p2 = new Product { Name = "Second", Price = 2m, StockQuantity = 2 };

        svc.AddProduct(p1);
        svc.AddProduct(p2);

        Assert.Equal(1, p1.Id);
        Assert.Equal(2, p2.Id);
    }

    [Fact]
    public void Continues_Id_Sequence_From_Existing_Metadata()
    {
        SeedMetadata(lastId: 5);
        var product = new Product { Name = "Next", Price = 1m, StockQuantity = 1 };
        CreateService().AddProduct(product);
        Assert.Equal(6, product.Id);
    }

    [Fact]
    public void Persists_Product_To_File()
    {
        CreateService().AddProduct(new Product { Name = "Saved", Price = 5m, StockQuantity = 3 });

        var raw = File.ReadAllText(Path.Combine(TestDir, "Inventory.json"));
        Assert.Contains("Saved", raw);
    }

    [Fact]
    public void Appends_To_Existing_Products()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Existing", Price = 1m, StockQuantity = 1 }
        });
        SeedMetadata(lastId: 1);

        CreateService().AddProduct(new Product { Name = "New", Price = 2m, StockQuantity = 2 });

        var raw = File.ReadAllText(Path.Combine(TestDir, "Inventory.json"));
        Assert.Contains("Existing", raw);
        Assert.Contains("New", raw);
    }

    [Fact]
    public void Throws_ArgumentNullException_When_Product_IsNull()
    {
        Assert.Throws<ArgumentNullException>(() => CreateService().AddProduct(null!));
    }

    [Fact]
    public void Throws_ArgumentException_When_StockQuantity_IsNegative()
    {
        var product = new Product { Name = "Bad", Price = 1m, StockQuantity = -1 };
        Assert.Throws<ArgumentException>(() => CreateService().AddProduct(product));
    }

    [Fact]
    public void Throws_ArgumentException_When_Price_IsNegative()
    {
        var product = new Product { Name = "Bad", Price = -0.01m, StockQuantity = 1 };
        Assert.Throws<ArgumentException>(() => CreateService().AddProduct(product));
    }

    [Fact]
    public void Accepts_Zero_Price()
    {
        var product = new Product { Name = "Free", Price = 0m, StockQuantity = 1 };
        Assert.Null(Record.Exception(() => CreateService().AddProduct(product)));
    }

    [Fact]
    public void Accepts_Zero_StockQuantity()
    {
        var product = new Product { Name = "OutOfStock", Price = 1m, StockQuantity = 0 };
        Assert.Null(Record.Exception(() => CreateService().AddProduct(product)));
    }
}

public class WarehouseService_UpdateProduct_Tests : WarehouseServiceTestBase
{
    [Fact]
    public void Updates_Name_Correctly()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Old Name", Price = 1m, StockQuantity = 1 }
        });

        CreateService().UpdateProduct(1, new Product { Name = "New Name", Price = 1m, StockQuantity = 1 });

        var raw = File.ReadAllText(Path.Combine(TestDir, "Inventory.json"));
        Assert.Contains("New Name", raw);
        Assert.DoesNotContain("Old Name", raw);
    }

    [Fact]
    public void Updates_Price_Correctly()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Item", Price = 1m, StockQuantity = 1 }
        });

        CreateService().UpdateProduct(1, new Product { Name = "Item", Price = 99.99m, StockQuantity = 1 });

        var raw = File.ReadAllText(Path.Combine(TestDir, "Inventory.json"));
        Assert.Contains("99.99", raw);
    }

    [Fact]
    public void Updates_StockQuantity_Correctly()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Item", Price = 1m, StockQuantity = 5 }
        });

        CreateService().UpdateProduct(1, new Product { Name = "Item", Price = 1m, StockQuantity = 100 });

        var raw = File.ReadAllText(Path.Combine(TestDir, "Inventory.json"));
        Assert.Contains("100", raw);
    }

    [Fact]
    public void Does_Not_Change_Id_When_Updating()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 7, Name = "Item", Price = 1m, StockQuantity = 1 }
        });

        CreateService().UpdateProduct(7, new Product { Name = "Updated", Price = 2m, StockQuantity = 2 });

        var products = JsonSerializer.Deserialize<List<Product>>(
            File.ReadAllText(Path.Combine(TestDir, "Inventory.json")))!;
        Assert.Equal(7, products[0].Id);
    }

    [Fact]
    public void Throws_KeyNotFoundException_When_Id_DoesNotExist()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Real", Price = 1m, StockQuantity = 1 }
        });

        Assert.Throws<KeyNotFoundException>(() =>
            CreateService().UpdateProduct(999, new Product { Name = "Ghost", Price = 1m, StockQuantity = 1 }));
    }

    [Fact]
    public void Throws_ArgumentNullException_When_Product_IsNull()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Real", Price = 1m, StockQuantity = 1 }
        });

        Assert.Throws<ArgumentNullException>(() => CreateService().UpdateProduct(1, null!));
    }

    [Fact]
    public void Throws_ArgumentException_When_Price_IsNegative()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Real", Price = 1m, StockQuantity = 1 }
        });

        Assert.Throws<ArgumentException>(() =>
            CreateService().UpdateProduct(1, new Product { Name = "Real", Price = -1m, StockQuantity = 1 }));
    }

    [Fact]
    public void Throws_ArgumentException_When_StockQuantity_IsNegative()
    {
        SeedInventory(new List<Product>
        {
            new Product { Id = 1, Name = "Real", Price = 1m, StockQuantity = 1 }
        });

        Assert.Throws<ArgumentException>(() =>
            CreateService().UpdateProduct(1, new Product { Name = "Real", Price = 1m, StockQuantity = -5 }));
    }
}