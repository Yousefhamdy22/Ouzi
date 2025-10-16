using Application.DTOs;
using Application.Services.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WembyResturant.Controllers
{
    [Route("api/Products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }


        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto request)
        {
            try
            {
                var product = await _productService.CreateProductAsync(request);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request while creating product");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while creating the product",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found: {ProductId}", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Product not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product: {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while fetching the product",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("category/{categoryId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for category: {CategoryId}", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while fetching products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("name/{name}")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsByName(string name)
        {
            try
            {
                var products = await _productService.GetProductsByNameAsync(name);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products by name: {Name}", name);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while fetching products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all products");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while fetching products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while searching products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableProducts()
        {
            try
            {
                var products = await _productService.GetAvailableProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available products");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while fetching products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet("vegetarian")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVegetarianProducts()
        {
            try
            {
                var products = await _productService.GetVegetarianProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vegetarian products");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while fetching products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet("queryable")]
        [ProducesResponseType(typeof(IQueryable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetProductsQueryable()
        {
            try
            {
                var query = _productService.GetProductsQueryable();
                return Ok(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queryable products");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while getting queryable products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }


        [HttpGet("filter")]
        [ProducesResponseType(typeof(IQueryable<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult FilterProducts(
            [FromQuery] ProductOrderingEnum ordering = ProductOrderingEnum.Name,
            [FromQuery] string searchTerm = null)
        {
            try
            {
                var query = _productService.FilterProductsQueryable(ordering, searchTerm);
                return Ok(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while filtering products",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                await _productService.UpdateProductAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for update: {ProductId}", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Product not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request while updating product: {ProductId}", id);
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while updating the product",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for deletion: {ProductId}", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Product not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while deleting the product",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("exists/{id:int}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProductExists(int id)
        {
            try
            {
                var exists = await _productService.ProductExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product existence: {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while checking product existence",
                    Status = StatusCodes.Status500InternalServerError
                });
            }





        }
    }
}
