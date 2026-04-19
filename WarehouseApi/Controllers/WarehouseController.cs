using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Models;
using WarehouseApi.Services;

namespace WarehouseApi.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class WarehouseController : ControllerBase
    {
        private readonly IManagementService _service;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(IManagementService service, ILogger<WarehouseController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found.", id);
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> Add(Product product)
        {
            await _service.AddProductAsync(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Product product)
        {
            try
            {
                await _service.UpdateProductAsync(id, product);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Cannot update product. ID {ProductId} was not found.", id);
                return NotFound();
            }
        }
    }
}