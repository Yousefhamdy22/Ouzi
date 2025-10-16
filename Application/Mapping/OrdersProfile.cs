using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using static Domain.Enums.PaymentMethodsResponse;

namespace Application.Mapping
{
    public class OrdersProfile : Profile
    {
        public OrdersProfile()
        {
            // 1. Order Entity to OrderDto
            CreateMap<Order, OrderDto>().ReverseMap();
            
            CreateMap<Order, OrderResponse>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber))
               .ForMember(dest => dest.DeliveryAddress, opt => opt.MapFrom(src => src.DeliveryAddress))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
               .ForMember(dest => dest.SpecialInstructions, opt => opt.MapFrom(src => src.SpecialInstructions))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
               .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Total))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.OrderDate))
               .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
               .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
               .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
               .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
               .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => MapPaymentMethodToString(src.PaymentMethodId)));
             
            // 3. OrderCreatedResponseDto to OrderResponse - FOR YOUR CONTROLLER
            CreateMap<OrderCreatedResponseDto, OrderResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseOrderStatus(src.Status)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                // All other properties need to come from the actual Order entity
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                //.ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())

                .ForMember(dest => dest.SpecialInstructions, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.TrackingNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore());

            // 4. OrderItem mappings
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

            // 5. Address mappings
            CreateMap<Address, AddressDto>().ReverseMap();

            // 6. Order to OrderCreatedResponseDto
            CreateMap<Order, OrderCreatedResponseDto>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.InvoiceUrl, opt => opt.MapFrom(src => src.InvoiceUrl))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.DocumentPath, opt => opt.MapFrom(src => src.DocumentPath));

            // Helper for CreateOrderDto if needed
            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                
                //.ForMember(dest => dest.SpecialInstructions, opt => opt.MapFrom(src => src.SpecialInstructions))
                .ForMember(dest => dest.DeliveryAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                // PaymentMethodId mapping - handle in service layer
                .ForMember(dest => dest.PaymentMethodId, opt => opt.Ignore())
                // Ignore all calculated and system-set properties
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
                .ForMember(dest => dest.Tax, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryFee, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceUrl, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryAddressId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
    
        }

        private static OrderStatus ParseOrderStatus(string status)
        {
            if (Enum.TryParse<OrderStatus>(status, out var result))
                return result;
            return OrderStatus.Pending;
        }

        private static string MapPaymentMethodToString(int paymentMethodId)
        {
            return paymentMethodId switch
            {
                1 => "CreditCard",
                2 => "PayPal",
                3 => "Wallet",
                4 => "CashOnDelivery",
                _ => "CashOnDelivery"
            };
        }

    }


    }
    
