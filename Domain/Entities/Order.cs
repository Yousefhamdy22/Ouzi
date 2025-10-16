
using Domain.Enums;
using Domain.Common;
using Domain.Rules.Orders;
using Domain.Events.Orders;
using static Domain.Enums.PaymentMethodsResponse;
using Domain.Events.Payment;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Order : Entity, IAggregateRoot
    {

        
        #region Properties 
        public int Id { get; private set; }
       
        public Guid UserId { get; private set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal Tax { get; private set; }
        public decimal DeliveryFee { get;  set; }
        public decimal Total { get; private set; }
        public OrderStatus Status { get; private set; }
        public string? InvoiceId { get; private set; }
        public string? InvoiceUrl { get; private set; }
        public PaymentStatus PaymentStatus { get; private set; }

       
        public int PaymentMethodId { get; private set; }
        public DateTime? PaymentDate { get; private set; }

        
        public int DeliveryAddressId { get; private set; }
        public Address DeliveryAddress { get; private set; }

      
        public string PhoneNumber { get; private set; }
        public string? Notes { get; private set; }
        public string? SpecialInstructions { get; private set; }
        public string? TrackingNumber { get; private set; }
        public string DocumentPath { get; private set; }
        #endregion

        #region Navigation
        //public ApplicationUser User { get;  set; }

        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        #endregion


       

       private string? _cancellationReason;
        private DateTime? _cancellationDate;



        public static Order Create(
              Guid userId,
              int orderNumber,
              Address? shippingAddress,
              string phoneNumber,
              int paymentMethodId,
              string? trackingNumber,
              string documentPath,
              string? notes,
              decimal subtotal,
              decimal deliveryFee, 
              decimal total
 )
        {
            var order = new Order
            {
                UserId = userId,
                OrderNumber = orderNumber,
                DeliveryAddress = shippingAddress,
                DeliveryAddressId = shippingAddress.Id,
                PhoneNumber = phoneNumber,
                PaymentMethodId = paymentMethodId,
                TrackingNumber = trackingNumber,
                DocumentPath = documentPath,
                Notes = notes,
                Subtotal = subtotal,              
                DeliveryFee = deliveryFee,  
                Total = total,
                OrderDate = DateTime.UtcNow,
                InvoiceId = null,
                InvoiceUrl = null,
                PaymentDate = null
            };

            // Debug check
            if (order.DeliveryFee == 0)
            {
                throw new InvalidOperationException($"DeliveryFee should not be 0! Expected: {deliveryFee}");
            }

            return order;
        }
     

        public void AddOrderItem(OrderItem item)
        {
            CheckRule(new CannotModifyOrderInFinalizedStatusRule(Status));

            var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(item.Quantity);
            }
            else
            {
                _orderItems.Add(item);
            }

            //RecalculateTotals();
        }

        public void Confirm(int orderId)
        {
            CheckRule(new OrderMustHaveAtLeastOneItemRule(_orderItems.Count));
            CheckRule(new CannotModifyOrderInFinalizedStatusRule(Status));
            Status = OrderStatus.Confirmed;
            RecalculateTotals();
            AddDomainEvent(new OrderConfirmedEvent(orderId));
        }

        public void Ship(string trackingNumber)
        {
            CheckRule(new OrderMustBePaidBeforeShippingRule(PaymentStatus));
            CheckRule(new OrderMustBeInProcessingStatusToShipRule(Status));
            CheckRule(new ShippingMustHaveTrackingNumberRule(trackingNumber));

            Status = OrderStatus.Confirmed;
            AddDomainEvent(new OrderShippedEvent(Id, trackingNumber, DateTime.UtcNow));
        }

        //public void Deliver()
        //{
        //    CheckRule(new OrderMustBeShippedBeforeDeliveryRule(Status));

        //    Status = OrderStatus.Delivered;
        //    AddDomainEvent(new OrderDeliveredEvent(Id, DateTime.UtcNow));
        //}

        public void Cancel(string reason)
        {
            CheckRule(new OrderCanOnlyBeCancelledInCertainStatusesRule(Status));
            CheckRule(new CancellationMustHaveReasonRule(reason));

            var previousStatus = Status;
            Status = OrderStatus.Cancelled;
            _cancellationReason = reason;
            _cancellationDate = DateTime.UtcNow;

            AddDomainEvent(new OrderCancelledEvent(Id, previousStatus, reason, DateTime.UtcNow));

            if (PaymentStatus == PaymentStatus.Paid)
            {
                CheckRule(new CannotCancelPaidOrderWithoutRefundRule(PaymentStatus));
                //PaymentStatus = PaymentStatus.RefundPending;
                AddDomainEvent(new RefundInitiatedEvent(Id, Total, reason));
            }
        }

        public void RemoveOrderItem(int productId)
        {
            CheckRule(new CannotRemoveItemFromFinalizedOrderRule(Status));

            var itemToRemove = _orderItems.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove == null)
                return;

            _orderItems.Remove(itemToRemove);

            if (_orderItems.Count == 0)
            {
                Cancel("All items were removed from the order");
            }
            else
            {
                CheckRule(new OrderMustHaveAtLeastOneItemRule(_orderItems.Count));
                RecalculateTotals();
                AddDomainEvent(new OrderItemRemovedEvent(Id, productId, itemToRemove.Quantity, DateTime.UtcNow));
            }
        }


        private void RecalculateTotals()
        {
            Subtotal = _orderItems.Sum(item => item.Price * item.Quantity);
            Tax = Subtotal * 0.08m;
            DeliveryFee = CalculateDeliveryFee();
            Total = Subtotal + Tax + DeliveryFee;

            if (_orderItems.Any() && Total <= 0)
                throw new Exception("Order total must be positive");
            //BusinessErrorHandling 
        }

        private decimal CalculateDeliveryFee()
        {
            return Subtotal > 100 ? 0 : 15.99m;
        }

        public void MarkAsPaid(string invoiceId, string invoiceUrl)
        {
            Status = OrderStatus.Completed;
            InvoiceId = invoiceId;
            InvoiceUrl = invoiceUrl;
            AddDomainEvent(new OrderPaidEvent(Id, invoiceId));
        }

        public void MarkAsFailed(string reason)
        {
            Status = OrderStatus.Failed;
            AddDomainEvent(new OrderFailedEvent(Id, reason));
        }

        public bool CanBeCreatedFromCart(Cart cart)
        {
            return cart != null && !cart.IsEmpty && cart.Items.All(item =>
                item.Quantity > 0 && item.UnitPrice > 0);
        }


        public void AddPaymentDetails(Enums.PaymentMethod paymentMethod, DateTime paymentDate)
        {
            //CheckRule(new OrderMustBeInPendingStatusToAddPaymentDetailsRule(Status));
            //this.PaymentMethodId = paymentMethod;
            PaymentDate = paymentDate;
            PaymentStatus = PaymentStatus.Paid;
            //UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new PaymentDetailsAddedEvent(Id, UserId, Total));
        }

        //public void ConfirmPayment(PaymentId paymentId)
        //{
        //    PaymentId = paymentId;
        //    PaymentStatus = PaymentStatus.Completed;
        //    Status = OrderStatus.Confirmed;
        //    UpdatedAt = DateTime.UtcNow;
        //    AddDomainEvent(new OrderConfirmedEvent(Id, UserId, TotalAmount));
        //}

        //public void FailPayment(string reason)
        //{
        //    PaymentStatus = PaymentStatus.Failed;
        //    Status = OrderStatus.Delivered;
        //    //crea = DateTime.UtcNow;
        //    AddDomainEvent(new OrderPaymentFailedEvent(Id, UserId, reason));
        //}
       
    }
}
