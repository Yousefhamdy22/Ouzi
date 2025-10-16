using Application.DTOs;
using Application.Exceptions;
using Application.Services.interfaces;
using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Application.Services.Implemntation
{
    public class CartServices : ICartServices
    {
       
        private readonly ICart _cartRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RestaurantDbContext _context;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<CartServices> _logger;


        public CartServices(ICart cartRepository, IUnitOfWork unitOfWork, IMapper mapper,
            IUserService userService, ILogger<CartServices> logger, RestaurantDbContext context)
        {
            _cartRepository = cartRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
            _context = context;
        }

        public async Task<CartDto> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            var cartDto = _mapper.Map<CartDto>(cart);

            return cartDto ?? throw new NotFoundException($"Cart not found for user", userId);
        }

        public async Task<Cart> CreateCartAsync(Guid userId)
        {
            try
            {
                var cart = Cart.Create(userId);
                await _cartRepository.AddAsync(cart);
                await _unitOfWork.CommitAsync();
                return cart;
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
              
                _logger.LogWarning("Cart already exists for user {UserId}, returning existing cart", userId);
                return await _cartRepository.GetByUserIdAsync(userId);
            }
        }
        private bool IsDuplicateKeyException(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2627 || sqlEx.Number == 2601); 
        }

        public async Task AddItemToCartAsync(CartItemDto cartItemDto)
        {
            await _unitOfWork.BeginTransactionAsync(); // Start transaction

            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(cartItemDto.ProductId);
                if (product == null)
                {
                    throw new NotFoundException("Product not found", cartItemDto.ProductId);
                }

                var cart = await GetOrCreateCartAsync(cartItemDto.UserId);

                // If cart is new, save it first to get the ID
                if (cart.Id == 0)
                {
                    await _cartRepository.AddAsync(cart);
                    await _unitOfWork.CommitAsync(); // Save to get ID
                }

                // Now add the item
                cart.AddItem(
                    productId: cartItemDto.ProductId,
                    productName: product.Name,
                    unitPrice: product.Price,
                    quantity: cartItemDto.Quantity,
                    imageUrl: product.ImageUrl
                );

                // EXPLICITLY update the cart repository
                await _cartRepository.UpdateAsync(cart);

                // Save changes AND commit transaction
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync(); // ← THIS IS CRITICAL
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }



        public async Task UpdateItemQuantityAsync(Guid userId, int productId, int newQuantity)
        {
            var cart = await GetCartByUserIdAsyncTransefar(userId); // Returns domain model
            cart.UpdateItemQuantity(productId, newQuantity);
            await _unitOfWork.CommitAsync();
        }


        public async Task<CartDto> GetCartDtoByUserIdAsync(Guid userId)
        {
            var cart = await GetCartByUserIdAsyncTransefar(userId);
            return cart == null
                ? throw new NotFoundException($"Cart not found for user", userId)
                : CartDto.FromDomain(cart);
        }

        // Internal method that returns domain model
        private async Task<Cart> GetCartByUserIdAsyncTransefar(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }


        //public async Task UpdateItemQuantityAsync(Guid userId, int productId, int newQuantity)
        //{

        //    //var userId = _userService.GetCurrentUserId();
        //    var cart = await GetCartByUserIdAsync(userId);
          
        //    cart.UpdateItemQuantity(productId, newQuantity);
        //    await _unitOfWork.CommitAsync();

         
        //}

        public async Task RemoveItemFromCartAsync(Guid userId, int productId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID");

            if (productId <= 0)
                throw new ArgumentException("Invalid product ID");

            var cart = await _cartRepository.GetByUserIdAsync(userId); // 
            if (cart == null)
                throw new ArgumentException($"Cart not found for user {userId}");

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem == null)
                throw new ArgumentException($"Product {productId} not found in cart");

            cart.RemoveItem(productId);

            var changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
                throw new InvalidOperationException("Failed to remove item from cart");

            _logger.LogInformation("Product {ProductId} removed from cart for user {UserId}", productId, userId);
        }
        public async Task ClearCartAsync(Guid userId)
        {
            // Get the domain model (Cart), not the DTO
            var cart = await _cartRepository.GetByUserIdAsync(userId);

            if (cart == null)
                throw new NotFoundException($"Cart not found for user", userId);

            // Use the domain model's business logic
            cart.Clear(); 

            // Mark as modified if using change tracking
           await _cartRepository.UpdateAsync(cart);

            await _unitOfWork.CommitAsync();
        }

       
        //public async Task ClearCartAsync(Guid userId)
        //{
        //    var cart = await GetCartByUserIdAsync(userId);
        //    //cart.Clear();
        //    await _unitOfWork.CommitAsync(); 
        //}

        public async Task<bool> CartExistsAsync(Guid userId)
        {
            return await _cartRepository.ExistsForUserAsync(userId);
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            return cart?.Items.Count ?? 0;
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = Cart.Create(userId);
                await _cartRepository.AddAsync(cart);
                // Don't commit here - let the calling method handle it
            }
            return cart;
        }

     
       

       
    }
}
