using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using Application.Services.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WembyResturant.Controllers
{
    [Route("api/Category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryServices _categoryServices;

        public CategoryController(ICategoryServices categoryServices)
        {
            _categoryServices = categoryServices;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDto categoryDto)
        {
            try
            {
                var response = await _categoryServices.CreateCategory(categoryDto);

                // Since your CreateCategory returns Response<Category>, not Response<CategoryDto>
                // We need to handle it appropriately
                if (response.Success && response.Data != null)
                {
                    return StatusCode(201, response); // 201 Created
                }

                return BadRequest(response);
            }
            catch (ServiceLayerException ex)
            {
                // Handle known business exceptions
                return BadRequest(Response<object>.Fail(ex.Message));
            }
           
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _categoryServices.DeleteAsync(id);

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(Response<object>.Fail(ex.Message));
            }
            catch (ServiceLayerException ex)
            {
                return BadRequest(Response<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<object>.Fail("An unexpected error occurred"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                // Ensure the ID in the route matches the ID in the DTO
                if (id != categoryDto.Id)
                {
                    return BadRequest(Response<object>.Fail("ID in route does not match ID in request body"));
                }

                var response = await _categoryServices.EditAsync(categoryDto);

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(Response<object>.Fail(ex.Message));
            }
            catch (DuplicateException ex)
            {
                return Conflict(Response<object>.Fail(ex.Message));
            }
            catch (ServiceLayerException ex)
            {
                return BadRequest(Response<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<object>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var response = await _categoryServices.GetByIdAsync(id);

                if (response.Success && response.Data != null)
                {
                    return Ok(response);
                }

                if (response.Success && response.Data == null)
                {
                    return NotFound(Response<object>.Fail("Category not found"));
                }

                return BadRequest(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(Response<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<object>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _categoryServices.GetCategoryListAsync();

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<object>.Fail("An unexpected error occurred"));
            }
        }

        // Additional endpoint for getting categories by name
        [HttpGet("search/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                // You'll need to implement this method in your service
                var response = await _categoryServices.GetByNameAsync();

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Response<object>.Fail("An unexpected error occurred"));
            }
        }
    }
}
