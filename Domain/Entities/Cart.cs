using Domain.Common;
using Domain.Enums;
using Domain.Events.Cart;
using Domain.Exceptions;
using Domain.Rules.Cart;
using Domain.Rules.CartItem;
using Domain.Rules.Orders;
using static Domain.Enums.PaymentMethodsResponse;

namespace Domain.Entities
{
    public class Cart : Entity, IAggregateRoot
    {

        #region Properites
        public int Id { get; set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        public decimal Total => _items.Sum(item => item.Quantity * item.UnitPrice);

        private Cart() { }
        #endregion

        #region Logic
        public static Cart Create(Guid userId)
        {
            CheckRule(new CartMustHaveUserIdRule(userId));

            return new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddItem(int productId, string productName, decimal unitPrice,
                     int quantity = 1, string imageUrl = null)
        {
            CheckRule(new CartItemMustHaveValidProductRule(productId, productName));
            CheckRule(new CartItemMustHavePositiveQuantityRule(quantity));
            CheckRule(new CartItemMustHavePositivePriceRule(unitPrice));

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity);
            }
            else
            {

                var newItem = CartItem.Create(0, productId, productName, unitPrice, quantity, imageUrl);

              
                newItem.Cart = this;  // Set the navigation property
                newItem.CartId = this.Id;
                _items.Add(newItem);
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItemQuantity(int productId, int newQuantity)
        {
            CheckRule(new CartItemMustHavePositiveQuantityRule(newQuantity));

            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                throw new NotFoundException($"Item with product ID {productId} not found in cart." , productId);

            item.UpdateQuantity(newQuantity);
            UpdatedAt = DateTime.UtcNow;
        
        }

        public void RemoveItem(int productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return;

            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
         
        }

        public void Clear()
        {
            _items.Clear();
            UpdatedAt = DateTime.UtcNow;
       
        }

        // Converts cart to order - this is where the magic happens
        public Order ConvertToOrder(int orderNumber , Address shippingAddress, string phoneNumber, Domain.Enums.PaymentMethod paymentMethod, string trackingnumber, string specialInstructions, string DocumentPath, string notes)
        {
            CheckRule(new CartMustHaveItemsRule(_items));
            CheckRule(new OrderMustHaveValidAddressRule(shippingAddress));
            CheckRule(new OrderMustHavePhoneNumberRule(phoneNumber));

            
            var orderItems = _items.Select(item =>
                OrderItem.Create(
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice, 
                    item.Quantity,
                    item.ImageUrl
                )
            ).ToList();

            // Calculate order totals
            decimal subtotal = orderItems.Sum(i => i.Price * i.Quantity);

            decimal deliveryFee = 0m;
            decimal total = subtotal + deliveryFee;

            // Create the order
            var order = Order.Create(
                UserId,
                orderNumber,
                
                shippingAddress,
                phoneNumber,
                (int)paymentMethod,
                trackingnumber,
                //specialInstructions,
                DocumentPath,
                notes,
                subtotal,
                
                deliveryFee,
                total
               
            );

            // Add all items to the order
            foreach (var orderItem in orderItems)
            {
                order.AddOrderItem(orderItem);
            }

            // Clear the cart after successful order creation
            Clear();

            AddDomainEvent(new CartConvertedToOrderEvent(Id, order.Id));

            return order;
        }

        public bool IsEmpty => !_items.Any();
        public int ItemCount => _items.Sum(item => item.Quantity);

        #endregion
    }
}
