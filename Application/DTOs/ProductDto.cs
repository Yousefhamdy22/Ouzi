using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        public string Size { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsVegetarian { get; set; }
        public int CategoryId { get; set; }

        public DateTime? CreatedDate { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Size { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVegetarian { get; set; }
        public int CategoryId { get; set; }
        //public string? CategoryName { get; set; }
        //public int StockQuantity { get; set; }

        public DateTime CreatedDate { get; set; }
    }
    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string ImageUrl { get; set; }
        public string Size { get; set; }
        public bool? IsAvailable { get; set; }
        public bool? IsVegetarian { get; set; }
        public int? CategoryId { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public enum ProductOrderingEnum
    {
        Id,
        Name,
        Price,
        Category,
        CreatedDate
    }

}
