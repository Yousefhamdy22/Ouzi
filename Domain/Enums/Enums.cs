
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed =2,
        Completed = 3,
        Failed = 4,
        Delivered =5,
        Cancelled = 6
    }
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentMethod
    {
        CashOnDelivery = 1,
        CreditCard = 2,      
        Wallet = 3,          
        PayPal = 4

    }
    public class JwtSettings
    { // Binding 
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiration { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }



    }
    public class EInvoiceRequestModel
    {

        [JsonProperty("payment_method_id")]
        public int? PaymentMethodId { get; set; }

        [JsonProperty("customer")]
        [Required]
        public required CustomerModel Customer { get; set; }


        [JsonProperty("cartItems")]
        [MinLength(1)]
        [Required]
        public List<CartItemModel> CartItems { get; set; }


        [JsonProperty("cartTotal")]
        public decimal CartTotal => CartItems.Sum(item => item.Price * item.Quantity);


        [JsonProperty("currency")]
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "EGP";


        [JsonProperty("payLoad")]
        public InvoicePayload? PayLoad { get; set; }

        //[JsonProperty("invoice_number")]
        //public string? InvoiceNumber { get; set; } // nrw


        //[JsonProperty("redirectionUrls")]
        //public RedirectionUrlsModel? RedirectionUrls { get; set; }


        public class InvoicePayload
        {
            public string OrderId { get; set; }
        }


        public class CustomerModel
        {

            [JsonProperty("customer_unique_id")]
            public string? CustomerId { get; set; }


            [JsonProperty("first_name")]
            [Required]
            public  string? FirstName { get; set; }


            [JsonProperty("last_name")]
            [Required]
            public  string? LastName { get; set; }


            [JsonProperty("email")]
            [EmailAddress]
            public string? Email { get; set; }


            [JsonProperty("phone")]
            [Phone]
            public string? Phone { get; set; }
        }


        public class CartItemModel
        {

            [JsonProperty("name")]
            [Required]
            public string Name { get; set; }

            [JsonProperty("price")]
            [Range(0.01, double.MaxValue)]
            public decimal Price { get; set; }

            [JsonProperty("quantity")]
            [Range(1, int.MaxValue)]
            public int Quantity { get; set; }
        }


        public class RedirectionUrlsModel
        {

            [JsonProperty("successUrl")]
            [Url]
            public string? OnSuccess { get; set; }

            [JsonProperty("failUrl")]
            [Url]
            public string? OnFailure { get; set; }

            [JsonProperty("pendingUrl")]
            [Url]
            public string? OnPending { get; set; }
        }
    }

    public class PaymentMethodsResponse
    {

        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("data")]
        public List<PaymentMethod> Data { get; set; }


        public class PaymentMethod
        {

            public int Id { get; set; }


            [JsonProperty("paymentId")]
            public int PaymentId { get; set; }


            [JsonProperty("name_en")]
            public string NameEn { get; set; }


            [JsonProperty("name_ar")]
            public string NameAr { get; set; }


            [JsonProperty("redirect")]
            public string Redirect { get; set; }


            [JsonProperty("logo")]
            public string Logo { get; set; }

        }
    }



}

