using Application.DTOs;
using Domain.Entities;

namespace Application.Services.interfaces
{
    public interface IOrderService
    {                         
        Task<Order> CreateOrderFromCartAsync(CreateOrderDto createOrderDto);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderDto>> GetAll();
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
        Task<Order> GetByIdWithItemsAsync(int orderId);
        Task ConfirmOrderAsync(int orderId);
        Task ProcessPaymentAsync(int orderId, string transactionId, decimal amountPaid);
        Task ShipOrderAsync(int orderId, string trackingNumber);
        Task CancelOrderAsync(int orderId, string reason);
        Task AddOrderItemAsync(int orderId, OrderItem item);
        Task RemoveOrderItemAsync(int orderId, int productId);

        Task ResetOrderNumber(int newStartNumber);
        //Task ResetOrderNumberSequence(int newStartNumber);
        Task<int> GenerateNextOrderNumber();

        Task ProcessPaymentCallbackAsync(string invoiceId, bool isSuccess, string failureReason = null);
    }
}
