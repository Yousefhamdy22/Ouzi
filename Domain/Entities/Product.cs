
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Domain.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public string? Size { get; set; }

        public string ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsAvailable { get; set; } = true;

        // navigation properties
        public int CategoryId { get; set; }
       
        [ForeignKey("CategoryId")]
        public  Category Category { get; set; }

      
        public  ICollection<OrderItem> OrderItems { get; set; }
        public  ICollection<CartItem> CartItems { get; set; }
    }
}
