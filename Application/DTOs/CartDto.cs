using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        //public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ItemCount => Items?.Count ?? 0;
        public bool IsEmpty => ItemCount == 0;
       
        public List<CartItemDto> Items { get; set; } = new();



        public static CartDto FromDomain(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,
                //UserId = cart.UserId,
                Items = cart.Items.Select(CartItemDto.FromDomain).ToList()
            };
        }
    }

    public class AddItemDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string ImageUrl { get; set; }
    }

    public class UpdateQuantityDto
    {
        [Required]
        public int ProductId { get; set; } // Remove UserId - get from auth context

        [Required]
        [Range(1, int.MaxValue)]
        public int NewQuantity { get; set; }
    }

    public class RemoveItemDto
    {
        [Required]
        public int ProductId { get; set; } // Remove UserId - get from auth context
    }
}
