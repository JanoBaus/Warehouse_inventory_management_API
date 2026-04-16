public interface IManagementService
{
    List<Product> GetAll();
    Product? GetById(int id);
    void AddProduct(Product product);
    void UpdateProduct(int id, Product product);
}