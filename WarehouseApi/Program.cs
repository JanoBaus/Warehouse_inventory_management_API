var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// TEST START: Warehouse management and product operations
var manager = new WarehouseManagement();
manager.ClearAll();

var chleba = new Product { Name = "Chleba", Price = 1.5m, Quantity = 100 };
var mléko = new Product { Name = "Mléko", Price = 0.99m, Quantity = 200 };
var vejce = new Product { Name = "Vejce", Price = 2.5m, Quantity = 50 };
var sýr = new Product { Name = "Sýr", Price = 3.0m, Quantity = 30 };
var máslo = new Product { Name = "Máslo", Price = 2.0m, Quantity = 20 };
var jogurt = new Product { Name = "Jogurt", Price = 0.75m, Quantity = 150 };
var kuře = new Product { Name = "Kuře", Price = 5.0m, Quantity = 10 };
var rýže = new Product { Name = "Rýže", Price = 1.0m, Quantity = 80 };
var těstoviny = new Product { Name = "Těstoviny", Price = 1.2m, Quantity = 60 };

manager.AddProduct(chleba);
manager.AddProduct(mléko);
manager.AddProduct(vejce);
manager.AddProduct(sýr);
manager.AddProduct(máslo);
manager.AddProduct(jogurt);
Console.WriteLine("Products added with ID: {0}", manager.GetAll().Where(p => p.Name.Equals("Jogurt", StringComparison.OrdinalIgnoreCase)).Select(p => p.Id).FirstOrDefault());
manager.AddProduct(kuře);
manager.AddProduct(rýže);
manager.AddProduct(těstoviny);

var allProducts = manager.GetAll();
Console.WriteLine("All Products:");
foreach (var product in allProducts)
{
    Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Price: {product.Price}, Quantity: {product.Quantity}");
}

Console.WriteLine("Product with ID 4: ");
var product4 = manager.GetById(4);
if (product4 != null)
{
    Console.WriteLine($"ID: {product4.Id}, Name: {product4.Name}, Price: {product4.Price}, Quantity: {product4.Quantity}");
}
else
{
    Console.WriteLine("Product with ID 4 not found.");
}

var upraveneVejce = new Product { Name = "Vejce", Price = 2.5m, Quantity = 100 };
manager.UpdateProduct(3, upraveneVejce);
Console.WriteLine("Updated Product with ID 3: ");
var updatedProduct3 = manager.GetById(3);
if (updatedProduct3 != null)
{
    Console.WriteLine($"ID: {updatedProduct3.Id}, Name: {updatedProduct3.Name}, Price: {updatedProduct3.Price}, Quantity: {updatedProduct3.Quantity}");
}
else
{
    Console.WriteLine("Product with ID 3 not found.");
}

var product5 = manager.GetById(5);
if (product5 != null)
{
    manager.DeleteProduct(5);
    Console.WriteLine("Deleted Product with ID 5.");
}
else
{
    Console.WriteLine("Product with ID 5 not found. Nothing deleted.");
}
// TEST END: Warehouse management and product operations

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
