using System.Text.Json;
using WarehouseApi.Models;
using WarehouseApi.Services;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace WarehouseApi.Tests
{
    public abstract class WarehouseServiceTestBase : IDisposable
    {
        protected readonly string TestDir;
        protected WarehouseService CreateService() => new WarehouseService(TestDir);

        protected WarehouseServiceTestBase()
        {
            TestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(TestDir);
        }

        protected void SeedInventory(List<Product> products)
        {
            File.WriteAllText(
                Path.Combine(TestDir, "Inventory.json"),
                JsonSerializer.Serialize(products)
            );
        }

        protected void SeedMetadata(int lastId)
        {
            File.WriteAllText(
                Path.Combine(TestDir, "WarehouseMetadata.json"),
                JsonSerializer.Serialize(new { LastProductId = lastId })
            );
        }

        public void Dispose()
        {
            Directory.Delete(TestDir, recursive: true);
        }
    }
}