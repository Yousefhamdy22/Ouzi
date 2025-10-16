using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CartItemDto
    {
        public int Id { get;  set; }
        public Guid UserId { get;  set; }
        public int ProductId { get;  set; }
        public string ProductName { get;  set; }
        public string? Description { get;  set; }
        public decimal UnitPrice { get;  set; }
        public string ImageUrl { get;  set; }
        public int Quantity { get;  set; }


        public static CartItemDto FromDomain(CartItem item)
        {
            return new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Description = item.Product?.Description

            };
        }
    }

    public class CartItemCreateDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal Total => UnitPrice * Quantity;


    }
}
