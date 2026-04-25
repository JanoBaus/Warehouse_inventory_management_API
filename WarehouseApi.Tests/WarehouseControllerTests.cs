using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WarehouseApi.Controllers;
using WarehouseApi.Models;
using WarehouseApi.Services;
using Xunit;

namespace WarehouseApi.Tests
{
    public abstract class WarehouseControllerTestBase
    {
        protected readonly Mock<IManagementService> MockService;
        protected readonly Mock<ILogger<WarehouseController>> MockLogger;
        protected readonly WarehouseController Controller;

        protected WarehouseControllerTestBase()
        {
            MockService = new Mock<IManagementService>();
            MockLogger = new Mock<ILogger<WarehouseController>>();
            Controller = new WarehouseController(MockService.Object, MockLogger.Object);
        }
    }

    public class WarehouseController_GetAll_Tests : WarehouseControllerTestBase
    {
        [Fact]
        public async Task Returns_200_With_ProductList()
        {
            MockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Product>
            {
                new Product { Id = 1, Name = "Widget", Price = 3m, StockQuantity = 7 }
            });

            var result = await Controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Single((List<Product>)ok.Value!);
        }

        [Fact]
        public async Task Returns_200_With_EmptyList_When_NoProducts()
        {
            MockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Product>());

            var result = await Controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Empty((List<Product>)ok.Value!);
        }
    }

    public class WarehouseController_Get_Tests : WarehouseControllerTestBase
    {
        [Fact]
        public async Task Returns_200_With_Product_When_Found()
        {
            var product = new Product { Id = 1, Name = "Widget", Price = 1m, StockQuantity = 1 };
            MockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

            var result = await Controller.Get(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(product, ok.Value);
        }

        [Fact]
        public async Task Returns_404_When_Product_NotFound()
        {
            MockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            var result = await Controller.Get(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }

    public class WarehouseController_Add_Tests : WarehouseControllerTestBase
    {
        [Fact]
        public async Task Returns_201_With_CreatedProduct()
        {
            var product = new Product { Name = "Sprocket", Price = 2m, StockQuantity = 10 };
            MockService.Setup(s => s.AddProductAsync(product))
                .Callback<Product>(p => p.Id = 1)
                .Returns(Task.CompletedTask);

            var result = await Controller.Add(product);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal(product, created.Value);
        }
    }

    public class WarehouseController_Update_Tests : WarehouseControllerTestBase
    {
        [Fact]
        public async Task Returns_204_On_Success()
        {
            var product = new Product { Id = 1, Name = "Updated", Price = 5m, StockQuantity = 2 };
            MockService.Setup(s => s.UpdateProductAsync(1, product)).Returns(Task.CompletedTask);

            var result = await Controller.Update(1, product);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Returns_404_When_Service_Throws_KeyNotFoundException()
        {
            var product = new Product { Id = 99, Name = "Ghost", Price = 1m, StockQuantity = 1 };
            MockService.Setup(s => s.UpdateProductAsync(99, product))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await Controller.Update(99, product);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
