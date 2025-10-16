using Application.DTOs;
using Application.Exceptions;
using Application.Services.interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WembyResturant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IOrderService _orderService;
        private readonly IOrderDocumentService _documentService;
        private readonly ILogger<OrderController> _logger;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IOrderDocumentService documentService,
           IMapper mapper, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
            _mapper = mapper;
            _documentService = documentService;
        }


        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderDto request)
        {

            try
            {
                var createOrderDto = _mapper.Map<CreateOrderDto>(request);
                var orderCreatedResponse = await _orderService.CreateOrderFromCartAsync(createOrderDto);
                var response = _mapper.Map<OrderResponse>(orderCreatedResponse);
                return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { error = "An error occurred while creating the order" });
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                // User For Claim Principal
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId.ToString() != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(MapOrderToResponse(order));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found");
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order");
                return StatusCode(500, new { error = "An error occurred while retrieving the order" });
            }
        }

        private OrderResponse MapOrderToResponse(Order order)
        {
            return _mapper.Map<OrderResponse>(order);
        }

        

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var orders = await _orderService.GetAll();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user orders");
                return StatusCode(500, new { error = "An error occurred while retrieving your orders" });
            }
        }




        [HttpDelete("{orderId}/items/{productId}")]
        public async Task<IActionResult> RemoveOrderItem(int orderId, int productId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                // Check if user owns this order
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (order.UserId.ToString() != userId)
                {
                    return Forbid();
                }

                await _orderService.RemoveOrderItemAsync(orderId, productId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Order or item not found");
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item");
                return StatusCode(500, new { error = "An error occurred while removing the order item" });
            }
        }


        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetUserOrders(Guid userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders.Select(MapOrderToResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user orders");
                return StatusCode(500, new { error = "An error occurred while retrieving user orders" });
            }
        }

        [HttpGet("{orderId}/download")]
        public async Task<IActionResult> DownloadOrderDocument(int orderId)
        {
            try
            {
                var order = await _orderService.GetByIdWithItemsAsync(orderId);

                var documentBytes = _documentService.GenerateOrderDocument(order);
                var fileName = $"Order_{orderId}_{DateTime.Now:yyyyMMdd}.docx";

                return File(documentBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
                return NotFound("Order not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating document for order: {OrderId}", orderId);
                return BadRequest($"Error generating document: {ex.Message}");
            }
        }



        [HttpPost("reset-numbering")]
        //[Authorize(Roles = "Admin")] // Secure this endpoint!
        public async Task<IActionResult> ResetOrderNumbering([FromQuery] int startFrom = 1)
        {
            try
            {
                await _orderService.ResetOrderNumber(startFrom);

                return Ok(new
                {
                    message = $"Order numbering will start from {startFrom}",
                    newStartNumber = startFrom
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting order numbering");
                return StatusCode(500, new { message = "Error resetting order numbering", error = ex.Message });
            }
        }
    }



    //[HttpGet("{id}")]
    //public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    //{
    //    try
    //    {
    //        var order = await _orderService.GetOrderByIdAsync(id);

    //        // User For Claim Principal
    //        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    //        if (order.UserId.ToString() != userId && !User.IsInRole("Admin"))
    //        {
    //            return Forbid();
    //        }

    //        return Ok(MapOrderToResponse(order));
    //    }
    //    catch (NotFoundException ex)
    //    {
    //        _logger.LogWarning(ex, "Order not found");
    //        return NotFound(new { error = ex.Message });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving order");
    //        return StatusCode(500, new { error = "An error occurred while retrieving the order" });

    //    }
    //}

    //private OrderResponse MapOrderToResponse(Order order)
    //{
    //    return _mapper.Map<OrderResponse>(order);
    //}






    //namespace WembyResturant.Controllers
    //    {
    //        [Route("api/Orders")]
    //        [ApiController]
    //        public class OrdersController : ControllerBase
    //        {

    //            private readonly IOrderService _orderService;
    //            private readonly IOrderDocumentService _documentService;
    //            private readonly ILogger<OrdersController> _logger;
    //            private readonly IMapper _mapper;

    //            public OrdersController(IOrderService orderService, IOrderDocumentService documentService,
    //               IMapper mapper, ILogger<OrdersController> logger)
    //            {
    //                _orderService = orderService;
    //                _logger = logger;
    //                _mapper = mapper;
    //                _documentService = documentService;
    //            }
    //            [HttpPost]
    //            public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderDto request)
    //            {
    //                try
    //                {
    //                    // Remove this line - request is already CreateOrderDto
    //                    // var createOrderDto = _mapper.Map<CreateOrderDto>(request);

    //                    // Call the service with the request directly
    //                    var orderCreatedResponse = await _orderService.CreateOrderFromCartAsync(request);

    //                    // Map the response from service to OrderResponse
    //                    var response = _mapper.Map<OrderResponse>(orderCreatedResponse);

    //                    return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
    //                }
    //                catch (AutoMapperMappingException mapEx)
    //                {
    //                    _logger.LogError(mapEx, "AutoMapper error creating order: {Message}", mapEx.Message);
    //                    return StatusCode(500, new { error = "Mapping error occurred while creating the order" });
    //                }
    //                catch (Exception ex)
    //                {
    //                    _logger.LogError(ex, "Error creating order");
    //                    return StatusCode(500, new { error = "An error occurred while creating the order" });
    //                }
    //            }

    //            // Alternative approach: If you need to return a complete OrderResponse with all data
    //            [HttpPost("complete")]
    //            public async Task<ActionResult<OrderResponse>> CreateOrderComplete([FromBody] CreateOrderDto request)
    //            {
    //                try
    //                {
    //                    // Create the order and get the created order entity
    //                    var orderCreatedResponse = await _orderService.CreateOrderFromCartAsync(request);

    //                    // If your service returns OrderCreatedResponseDto, but you need full OrderResponse,
    //                    // you might need to fetch the complete order data
    //                    var completedOrder = await _orderService.GetOrderByIdAsync(orderCreatedResponse.Id);

    //                    // Map the complete order to OrderResponse
    //                    var response = _mapper.Map<OrderResponse>(completedOrder);

    //                    return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
    //                }
    //                catch (Exception ex)
    //                {
    //                    _logger.LogError(ex, "Error creating order");
    //                    return StatusCode(500, new { error = "An error occurred while creating the order" });
    //                }
    //            }

    //            //[HttpPost]
    //            //public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderDto request)
    //            //{

    //            //    try
    //            //    {
    //            //        var createOrderDto = _mapper.Map<CreateOrderDto>(request);
    //            //        var orderCreatedResponse = await _orderService.CreateOrderFromCartAsync(createOrderDto);
    //            //        var response = _mapper.Map<OrderResponse>(orderCreatedResponse);
    //            //        return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
    //            //    }
    //            //    catch (Exception ex)
    //            //    {
    //            //        _logger.LogError(ex, "Error creating order");
    //            //        return StatusCode(500, new { error = "An error occurred while creating the order" });
    //            //    }

    //            //}
    //            //[HttpGet("{id}")]
    //            //public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    //            //{
    //            //    try
    //            //    {
    //            //        var order = await _orderService.GetOrderByIdAsync(id);

    //            //        // User For Claim Principal
    //            //        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    //            //        if (order.UserId.ToString() != userId && !User.IsInRole("Admin"))
    //            //        {
    //            //            return Forbid();
    //            //        }

    //            //        return Ok(MapOrderToResponse(order));
    //            //    }
    //            //    catch (NotFoundException ex)
    //            //    {
    //            //        _logger.LogWarning(ex, "Order not found");
    //            //        return NotFound(new { error = ex.Message });
    //            //    }
    //            //    catch (Exception ex)
    //            //    {
    //            //        _logger.LogError(ex, "Error retrieving order");
    //            //        return StatusCode(500, new { error = "An error occurred while retrieving the order" });

    //            //    }
    //            //}

    //            //private OrderResponse MapOrderToResponse(Order order)
    //            //{
    //            //    return _mapper.Map<OrderResponse>(order);
    //            //}

    //            //[HttpGet("{id}")] 
    //            //public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    //            //{
    //            //    try
    //            //    {
    //            //        _logger.LogInformation("GetOrder request for ID: {OrderId}", id);


    //            //        if (!User.Identity.IsAuthenticated)
    //            //        {
    //            //            _logger.LogWarning("User is not authenticated");
    //            //            return Unauthorized();
    //            //        }

    //            //        var order = await _orderService.GetOrderByIdAsync(id);
    //            //        _logger.LogInformation("Order found: {OrderId}, UserId: {OrderUserId}", order.Id, order.UserId);

    //            //        // Get current user info
    //            //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //            //        var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

    //            //        _logger.LogInformation("Current user: {UserId}, Roles: [{Roles}]",
    //            //            userId, string.Join(", ", userRoles));

    //            //        if (string.IsNullOrEmpty(userId))
    //            //        {
    //            //            _logger.LogWarning("User ID claim not found in token");
    //            //            return Forbid("User ID not found in token");
    //            //        }

    //            //        // FIXED: Proper authorization check
    //            //        var orderUserId = order.UserId?.ToString();
    //            //        bool isOwner = string.Equals(orderUserId, userId, StringComparison.OrdinalIgnoreCase);
    //            //        bool isAdmin = User.IsInRole("Admin");

    //            //        _logger.LogInformation("Authorization - IsOwner: {IsOwner}, IsAdmin: {IsAdmin}", isOwner, isAdmin);

    //            //        if (!isOwner && !isAdmin)
    //            //        {
    //            //            _logger.LogWarning("Access denied. User {UserId} tried to access order {OrderId}", userId, id);
    //            //            return Forbid("You don't have permission to access this order");
    //            //        }

    //            //        return Ok(MapOrderToResponse(order));
    //            //    }
    //            //    catch (NotFoundException ex)
    //            //    {
    //            //        _logger.LogWarning(ex, "Order not found for ID: {OrderId}", id);
    //            //        return NotFound(new { error = ex.Message });
    //            //    }
    //            //    catch (Exception ex)
    //            //    {
    //            //        _logger.LogError(ex, "Error retrieving order {OrderId}", id);
    //            //        return StatusCode(500, new { error = "An error occurred while retrieving the order" });
    //            //    }
    //            //}







    //        }


}

