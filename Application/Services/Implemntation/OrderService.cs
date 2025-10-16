#region Using
using Application.DTOs;
using Application.DTOs.Payment;
using Application.Exceptions;
using Application.Options;
using Application.Services.interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;



#endregion

namespace Application.Services.Implemntation
{
    public class OrderService : IOrderService
    {

        #region DI

       
        private readonly IGenaricRepository<Order> _orderRepository;
        private readonly RestaurantDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrder _order;
        private readonly IUserService _userService;
        private readonly IProductService _productService; 
        private readonly IAddressRepository _addressRepository; 
        private readonly ICategoryServices _categoryService; 
        private readonly ICart _cartRepository; 
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IOptions<FawaterakOptions> _config; 
        private readonly IFawaterakPaymentService _paymentService;
        private readonly IWebHostEnvironment _environment;
        private readonly IOrderDocumentService _documentService;
        #endregion

        #region Constr 


        public OrderService(
             IGenaricRepository<Order> orderRepository,
             IProductService productService,
             IFawaterakPaymentService paymentService,
             ICategoryServices categoryServices,
             IUnitOfWork unitOfWork,
             IOrder order,
             ICart cartRepository,
             IUserService userService,
             IAddressRepository addressRepository,
             IMapper mapper,
             RestaurantDbContext context,
                UserManager<ApplicationUser> userManager,   
             IOptions<FawaterakOptions> config,
             ILogger<OrderService> logger,
             IOrderDocumentService documentService,
             IWebHostEnvironment environment
           )
        {
            _orderRepository = orderRepository;
            _order = order;
            _productService = productService;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _categoryService = categoryServices;
            _cartRepository = cartRepository;
            _paymentService = paymentService;
            _addressRepository = addressRepository;
            _logger = logger;
            _config = config;
            _mapper = mapper;
            _documentService = documentService;
            _environment = environment;
            _context = context;
            _userManager = userManager;

        }

        #endregion

        #region CreateOrder

      
        public async Task<Order> CreateOrderFromCartAsync(CreateOrderDto createOrderDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Validate cart
                var cart = await ValidateCart(createOrderDto.UserId);

                // 2. Create cart snapshot
                var cartItemsSnapshot = CreateCartSnapshot(cart);
                var totalAmount = CalculateTotalAmount(cartItemsSnapshot);

                // 3. Create order
                var newOrder = await CreateOrder(createOrderDto, cartItemsSnapshot);

                // 4. Create invoice
                var invoiceResponse = await CreateInvoice(newOrder, cart, createOrderDto);

                // 5. Update order with payment details
                await UpdateOrderPaymentDetails(newOrder, createOrderDto.PaymentMethod);

                

                // 7. Generate and save document
                var documentPath = await GenerateAndSaveOrderDocument(newOrder);

                // 7. EXPLICITLY SAVE CHANGES BEFORE COMMITTING
                await _unitOfWork.CommitAsync();

                // 8. Load the complete order with items before returning
                var completeOrder = await _order.GetByIdWithItemsAsync(newOrder.Id);

                // 9. Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                return completeOrder;

               
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        #region MethodHelepr

        private async Task<Cart> ValidateCart(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdWithItemsAsync(userId);
            if (cart == null || cart.IsEmpty)
                throw new Exception("Cart is empty or not found");

            if (!cart.Items.All(item => item.Quantity > 0 && item.UnitPrice > 0))
                throw new Exception("Cart contains invalid items");

            return cart;
        }

        private List<CartItemCreateDto> CreateCartSnapshot(Cart cart)
        {
            return cart.Items.Select(item => new CartItemCreateDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            }).ToList();
        }

        private decimal CalculateTotalAmount(List<CartItemCreateDto> cartItems)
        {
           
            return cartItems.Sum(item => item.UnitPrice * item.Quantity);
        }

       


        private async Task<Order> CreateOrder(CreateOrderDto createOrderDto, 
            List<CartItemCreateDto> cartItemsSnapshot)
        {
            
            string userIdString = createOrderDto.UserId.ToString().ToLower();

            if (createOrderDto.ShippingAddress == null)
            {
                throw new InvalidOperationException(
                    "Shipping address is required because this user has no saved addresses.");
            }

            var shippingAddress = await GetOrCreateShippingAddressAsync(
                userIdString,
                createOrderDto.ShippingAddress
            );



            var subtotal = cartItemsSnapshot.Sum(item => item.UnitPrice * item.Quantity);
            //var tax = subtotal * 0.1m;
            var deliveryFee = createOrderDto.DeliveryFee;
            var total = subtotal +  deliveryFee;


            var orderNumber = await GenerateNextOrderNumber();
            Console.WriteLine($"Generated Order Number: {orderNumber}");
            var newOrder = Order.Create(
                createOrderDto.UserId,
                orderNumber,
                shippingAddress,
                createOrderDto.PhoneNumber,
                MapToPaymentMethodId(MapToDomainPaymentMethod(createOrderDto.PaymentMethod)),
                null, 
                "",
                createOrderDto.Notes,
                subtotal,
                deliveryFee,  
                total

            );
            Console.WriteLine($"After Order.Create - DeliveryFee: {newOrder.DeliveryFee}, Total: {newOrder.Total}");

            foreach (var cartItemData in cartItemsSnapshot)
            {
                var orderItem = OrderItem.Create(
                    cartItemData.ProductId,
                    cartItemData.ProductName,
                    cartItemData.UnitPrice,
                    cartItemData.Quantity,
                    cartItemData.ImageUrl
                );
                newOrder.AddOrderItem(orderItem);
            }

            Console.WriteLine($"Before AddAsync - DeliveryFee: {newOrder.DeliveryFee}, Total: {newOrder.Total}");

            await _orderRepository.AddAsync(newOrder);

            Console.WriteLine($"Before AddAsync - DeliveryFee: {newOrder.DeliveryFee}, Total: {newOrder.Total}");
            await _unitOfWork.CommitAsync();
            return newOrder;
        }




        private async Task<Address> GetOrCreateShippingAddressAsync(string userId, 
            AddressDto? shippingAddressDto)
        {
            // 1️⃣ First, ensure the user exists
            //var user = await _userManager.FindByIdAsync(userId);
            //if (user == null)
            //{
            //    throw new InvalidOperationException($"User with ID {userId} does not exist.");
            //}

           
            if (shippingAddressDto != null)
            {
               
                var existingAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a =>
                        a.UserId == userId &&
                        a.Street == shippingAddressDto.Street &&
                        a.City == shippingAddressDto.City);

                if (existingAddress != null)
                    return existingAddress;

              
                var newAddress = new Address
                {
                    UserId = userId, 
                    Street = shippingAddressDto.Street,
                    City = shippingAddressDto.City
                };

                await _addressRepository.AddAsync(newAddress);
                return newAddress;
            }

         
            var existingUserAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (existingUserAddress == null)
            {
               
                throw new InvalidOperationException(
                    $"User {userId} has no addresses. Please provide a shipping address.");
            }

            return existingUserAddress;
        }
      
        private int MapToPaymentMethodId(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.CreditCard => 1,
                PaymentMethod.PayPal => 2,
                PaymentMethod.Wallet => 3,
                PaymentMethod.CashOnDelivery => 4,
                _ => 1 
            };
        }

        private async Task<EInvoiceResponseModel> CreateInvoice(Order newOrder, Cart cart, 
            CreateOrderDto createOrderDto)
        {
            var orderDto = _mapper.Map<OrderDto>(newOrder);
            var einvoiceRequest = MapOrderToEInvoiceRequest(orderDto, cart, createOrderDto);

            var invoiceResponse = await _paymentService.CreateEInvoiceAsync(einvoiceRequest);
            if (invoiceResponse == null)
                throw new Exception("Failed to create invoice: No response from payment service.");

            //return invoiceResponse;

            return new EInvoiceResponseModel
            {
                Status = invoiceResponse.Url,
                Data = invoiceResponse
            };
        }

        #region MapToDomainPaymentMethod
        private Domain.Enums.PaymentMethod MapToDomainPaymentMethod(string paymentMethodName)
        {
            if (string.IsNullOrEmpty(paymentMethodName))
                return Domain.Enums.PaymentMethod.CashOnDelivery;

            return paymentMethodName.ToLowerInvariant() switch
            {
                "creditcard" or "card" => Domain.Enums.PaymentMethod.CreditCard,
                "paypal" => Domain.Enums.PaymentMethod.PayPal,
                "wallet" => Domain.Enums.PaymentMethod.Wallet,
                "cashondelivery" or "cash" => Domain.Enums.PaymentMethod.CashOnDelivery,
                _ => Domain.Enums.PaymentMethod.CashOnDelivery
            };
        }
      
        #endregion

        private async Task UpdateOrderPaymentDetails(Order order, string paymentMethod)
        {
            
            var domainPaymentMethod = MapToDomainPaymentMethod(paymentMethod);

            order.AddPaymentDetails(domainPaymentMethod, DateTime.UtcNow);
            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();
        }

        private async Task<string> GenerateAndSaveOrderDocument(Order newOrder)
        {
            var orderDocument = _documentService.GenerateOrderDocument(newOrder);

            var fileName = $"Order_{newOrder.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
            var folderPath = Path.Combine(_environment.WebRootPath, "OrderDocuments");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(filePath, orderDocument);

            return $"/OrderDocuments/{fileName}"; 
        }

        #endregion

        #region MappingForAPIIntegraation


        private EInvoiceRequestModel MapOrderToEInvoiceRequest(OrderDto order, Cart cart, CreateOrderDto createOrderDto)
        {
            // 1. Build the Customer Model
            var customerModel = new EInvoiceRequestModel.CustomerModel
            {
                CustomerId = createOrderDto.UserId.ToString(),
                FirstName = createOrderDto.Customer.FirstName,
                LastName = createOrderDto.Customer.LastName,
                Email = GetUserEmail(createOrderDto.UserId), 
                Phone = createOrderDto.PhoneNumber
            };

            // 2. Build the Cart Items Model
            var cartItemsModel = cart.Items.Select(item => new EInvoiceRequestModel.CartItemModel
            {
                Name = item.ProductName,
                Price = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList();

            // 3. Build the Payload
            var payload = new EInvoiceRequestModel.InvoicePayload
            {
                OrderId = order.Id.ToString()
            };

            // 4. Construct and return the complete E-Invoice Request
            var einvoiceRequest = new EInvoiceRequestModel
            {
                Customer = customerModel,
                CartItems = cartItemsModel,
                Currency = "EGP",
                PayLoad = payload,
                PaymentMethodId = createOrderDto.PaymentMethodDetails != null 
                    ? MapToFawaterakPaymentMethod(createOrderDto.PaymentMethodDetails) 
                    : null
            };

            return einvoiceRequest;
        }
        private string GetUserEmail(Guid userId)
        {
            return "user@example.com"; 
        }
        private int? MapToFawaterakPaymentMethod(PaymentMethodsResponse.PaymentMethod paymentMethod)
        {
            
            return paymentMethod.NameEn.ToLower() switch
            {
                "card" => 1,
                "vodafone_cash" => 2,
                "paypal" => 3,
                "wallet" => 4,
                _ => null 
            };
        }
        #endregion


        #endregion

        #region Logic


        #region ResetOrderNum

       
        private static int _customStartNumber = 0; 

        public async Task ResetOrderNumber(int newStartNumber = 1)
        {
            _customStartNumber = newStartNumber;
            _logger.LogInformation("Order numbering reset to start from: {StartNumber}", newStartNumber);
        }

        public async Task<int> GenerateNextOrderNumber()
        {
            // If custom start number is set, use it
            if (_customStartNumber > 0)
            {
                var nextNumber = _customStartNumber;
                _customStartNumber++; // Increment for next order
                return nextNumber;
            }

            // Normal behavior - get max from database
            var maxOrderNumber = await _context.Orders
                .MaxAsync(o => (int?)o.OrderNumber) ?? 0;

            return maxOrderNumber + 1;
        }
        

        #endregion



        public async Task<Order> GetByIdWithItemsAsync(int orderId)
        {
            var OrderWithItems = await _order.GetByIdWithItemsAsync(orderId);
            if (OrderWithItems == null)
                throw new NotFoundException($"Order with ID {orderId} not found.", orderId);

            return OrderWithItems;

        }

        public async Task ProcessPaymentCallbackAsync(string invoiceId, bool isSuccess, string failureReason = null)
        {
            var order = await _order.GetByInvoiceIdAsync(invoiceId);
            if (order == null)
                throw new NotFoundException($"Order with invoice ID {invoiceId} not found" , invoiceId);

            if (isSuccess)
            {
                order.MarkAsPaid(invoiceId, order.InvoiceUrl);
            }
            else
            {
                order.MarkAsFailed(failureReason ?? "Payment failed");
            }

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new NotFoundException($"Order with ID {orderId} not found." , orderId);

            return order;
        }

        public  Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
        {
            //return  _order.GetByUserIdAsync(userId);
            return Task.FromResult<IEnumerable<Order>>(new List<Order> { _order.GetByUserIdAsync(userId).Result });
        }

        public async Task ConfirmOrderAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            order.Confirm(orderId);

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

         
        }

    
        public async Task ShipOrderAsync(int orderId, string trackingNumber)
        {
            var order = await GetOrderByIdAsync(orderId);
            order.Ship(trackingNumber);

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

          
        }

        public async Task CancelOrderAsync(int orderId, string reason)
        {
            var order = await GetOrderByIdAsync(orderId);
            order.Cancel(reason);

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

         


        }

        public async Task AddOrderItemAsync(int orderId, OrderItem item)
        {
            var order = await GetOrderByIdAsync(orderId);

            // Check product availability
            await _productService.CheckProductAvailabilityAsync(item.ProductId, item.Quantity);

            order.AddOrderItem(item);

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();
        }

        public async Task RemoveOrderItemAsync(int orderId, int productId)
        {
            var order = await GetOrderByIdAsync(orderId);
            order.RemoveOrderItem(productId);

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();
        }

        public Task ProcessPaymentAsync(int orderId, string transactionId, decimal amountPaid)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderDto>> GetAll()
        {
           var orders = await _order.GetAllAsync();
            if (orders == null || !orders.Any())
                throw new Exception("No orders found.");
            var orderDtos = _mapper.Map<List<OrderDto>>(orders);
            return orderDtos;
        }


        #endregion
    }
}