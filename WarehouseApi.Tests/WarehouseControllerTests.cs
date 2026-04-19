using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public abstract class WarehouseControllerTestBase
{
    protected readonly Mock<IManagementService> MockService;
    protected readonly WarehouseController Controller;

    protected WarehouseControllerTestBase()
    {
        MockService = new Mock<IManagementService>();
        Controller  = new WarehouseController(MockService.Object);
    }
}

public class WarehouseController_GetAll_Tests : WarehouseControllerTestBase
{
    [Fact]
    public void Returns_200_With_ProductList()
    {
        MockService.Setup(s => s.GetAll()).Returns(new List<Product>
        {
            new Product { Id = 1, Name = "Widget", Price = 3m, StockQuantity = 7 }
        });

        var result = Controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single((List<Product>)ok.Value!);
    }

    [Fact]
    public void Returns_200_With_EmptyList_When_NoProducts()
    {
        MockService.Setup(s => s.GetAll()).Returns(new List<Product>());

        var result = Controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Empty((List<Product>)ok.Value!);
    }
}

public class WarehouseController_Get_Tests : WarehouseControllerTestBase
{
    [Fact]
    public void Returns_200_With_Product_When_Found()
    {
        var product = new Product { Id = 1, Name = "Widget", Price = 1m, StockQuantity = 1 };
        MockService.Setup(s => s.GetById(1)).Returns(product);

        var result = Controller.Get(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(product, ok.Value);
    }

    [Fact]
    public void Returns_404_When_Product_NotFound()
    {
        MockService.Setup(s => s.GetById(99)).Returns((Product?)null);

        var result = Controller.Get(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}

public class WarehouseController_Add_Tests : WarehouseControllerTestBase
{
    [Fact]
    public void Returns_201_With_CreatedProduct()
    {
        var product = new Product { Name = "Sprocket", Price = 2m, StockQuantity = 10 };
        MockService.Setup(s => s.AddProduct(product))
                   .Callback<Product>(p => p.Id = 1);

        var result = Controller.Add(product);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(product, created.Value);
    }

    [Fact]
    public void Returns_400_When_Service_Throws_ArgumentException()
    {
        var product = new Product { Name = "Bad", Price = -1m, StockQuantity = 1 };
        MockService.Setup(s => s.AddProduct(product))
                   .Throws(new ArgumentException("Price cannot be negative."));

        var result = Controller.Add(product);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Price cannot be negative.", badRequest.Value);
    }
}

public class WarehouseController_Update_Tests : WarehouseControllerTestBase
{
    [Fact]
    public void Returns_204_On_Success()
    {
        var product = new Product { Name = "Updated", Price = 5m, StockQuantity = 2 };
        MockService.Setup(s => s.UpdateProduct(1, product));

        var result = Controller.Update(1, product);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Returns_404_When_Service_Throws_KeyNotFoundException()
    {
        var product = new Product { Name = "Ghost", Price = 1m, StockQuantity = 1 };
        MockService.Setup(s => s.UpdateProduct(99, product))
                   .Throws(new KeyNotFoundException());

        var result = Controller.Update(99, product);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Returns_400_When_Service_Throws_ArgumentException()
    {
        var product = new Product { Name = "Bad", Price = 1m, StockQuantity = -5 };
        MockService.Setup(s => s.UpdateProduct(1, product))
                   .Throws(new ArgumentException("Stock quantity cannot be negative."));

        var result = Controller.Update(1, product);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Stock quantity cannot be negative.", badRequest.Value);
    }
}