using Application.DTOs;
using Application.Services.Implemntation;
using Application.Services.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace WembyResturant.Controllers
{
    [Route("api/Cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartServices _cartService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public CartController(ICartServices cartService , IMapper mapper, IUserService userService)
        {
            _cartService = cartService;
            _mapper = mapper;
            _userService = userService;
        }



        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromBody] CartItemDto request)
        {

          await _cartService.AddItemToCartAsync(request);
            return Ok(request);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CartDto>> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartDtoByUserIdAsync(userId);      
            return Ok(cart);
        }

        //[HttpPut("items/{productId}/quantity")]
        //public async Task<IActionResult> UpdateQuantity(Guid userId, int productId, [FromBody] int newQuantity)
        //{
        //    await _cartService.UpdateItemQuantityAsync(userId, productId, newQuantity);
        //    return Ok();
        //}

        [HttpPut("items/quantity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateQuantity([FromQuery] Guid Userid, UpdateQuantityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _cartService.UpdateItemQuantityAsync(Userid, dto.ProductId, dto.NewQuantity);
            return Ok();
        }

       
        [HttpDelete("items/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                // Get current user ID
                var userId = _userService.GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                if (productId <= 0)
                    return BadRequest("Invalid product ID");

                await _cartService.RemoveItemFromCartAsync(userId, productId);
                return Ok(new { message = "Item removed successfully" });
            }
            catch (ArgumentException ex)
            {
             
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
              
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
            
                return StatusCode(500, "An internal error occurred");
            }
        }

        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _cartService.ClearCartAsync(userId);
            return Ok();
        }

        [HttpGet("{userId}/count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetItemCount(Guid userId)
        {
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(count);
        }

       
    }
}
