using Application.DTOs;
using Domain.Entities;


namespace Application.Services.interfaces
{

    public interface ICartServices
    {

        Task<CartDto> GetCartByUserIdAsync(Guid userId);
        Task<CartDto> GetCartDtoByUserIdAsync(Guid userId);
        Task<Cart> CreateCartAsync(Guid userId);
        Task AddItemToCartAsync(CartItemDto cartItemDto);
        Task UpdateItemQuantityAsync(Guid userId, int productId, int newQuantity);
        Task RemoveItemFromCartAsync(Guid userId, int productId);
        Task ClearCartAsync(Guid userId);
        Task<bool> CartExistsAsync(Guid userId);
        Task<int> GetCartItemCountAsync(Guid userId);

    }
}
