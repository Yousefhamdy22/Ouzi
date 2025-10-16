
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static Domain.Enums.EInvoiceRequestModel;
using static Domain.Enums.PaymentMethodsResponse;


namespace Application.DTOs
{
    public class OrderDto
    {  
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public List<OrderItemRequest> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal DeliveryFee { get; set; }
        public DateTime creeateDate { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderCreatedResponseDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string InvoiceUrl { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string DocumentPath { get; set; }

    }

   

    public class CreateOrderDto
    {
        #region Core Order Information

        public Guid UserId { get; set; }

        [Required]
        public CustomerModel Customer { get; set; } 

        [Required]
        public AddressDto ShippingAddress { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CartItemCreateDto> Items { get; set; }

        #endregion

        #region Payment & Amount Details (NEW - Critical for Fawaterak)

       
        [Required]
        public string Currency { get; set; } = "EGP";
        public decimal DeliveryFee { get; set; } 

      

        //[Required]
        //public string? SuccessUrl { get; set; } 

        //[Required]
        //public string? CancelUrl { get; set; }

        #endregion

        #region Optional Order Details (Keep your existing fields)

     
        public string? Notes { get; set; }

        
        public string PhoneNumber => Customer?.Phone; 

        #endregion

        #region Fields to REMOVE (or handle differently)

       
        public string PaymentMethod { get; set; } 

        [JsonIgnore]
        public PaymentMethodsResponse.PaymentMethod? PaymentMethodDetails { get; set; }

        #endregion

        public CreateOrderDto()
        {
            Items = new List<CartItemCreateDto>();
        }
    }

    
   
    public class OrderResponse
    {
        #region Attributes
        public int Id { get; init; }
        public Guid UserId { get; init; }
        public int OrderNumber { get; set; }
        public AddressDto DeliveryAddress { get; init; }
        public string PhoneNumber { get; init; }
        public string PaymentMethod { get; init; }
        public string? SpecialInstructions { get; init; }
        public OrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<OrderItemResponse> Items { get; init; }
        public string? Notes { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? TrackingNumber { get; init; }
        public string? CancellationReason { get; init; }

        #endregion

        #region Ctors

       

        public OrderResponse()
        {
            Items = new List<OrderItemResponse>();
        }

       
        public OrderResponse(
            int id, Guid userId, AddressDto deliveryAddress, string phoneNumber,
           string paymentMethod, OrderStatus status,
            decimal totalAmount, DateTime createdAt, List<OrderItemResponse> items,
           string? cancellationReason = null, decimal deliveryFee = 0)
        {
            Id = id;
            UserId = userId;
            DeliveryAddress = deliveryAddress;
            PhoneNumber = phoneNumber;
            PaymentMethod = paymentMethod;
           
            Status = status;
            TotalAmount = totalAmount;
            CreatedAt = createdAt;
            Items = items ?? new List<OrderItemResponse>();
        
            CancellationReason = cancellationReason;
            DeliveryFee = deliveryFee;
        }

        #endregion
    }

    public record ShipOrderRequest(string TrackingNumber);
    public record CancelOrderRequest(string Reason);

}
