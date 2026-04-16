using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class WarehouseController : ControllerBase
{
    private readonly IManagementService _service;

    public WarehouseController(IManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Product> Get(int id)
    {
        var product = _service.GetById(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public ActionResult Add(Product product)
    {
        try
        {
            _service.AddProduct(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, Product product)
    {
        try
        {
            _service.UpdateProduct(id, product);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}