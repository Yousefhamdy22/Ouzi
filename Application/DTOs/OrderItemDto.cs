using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
  
    public record OrderItemRequest(
       [Required] int ProductId,
       [Required] string ProductName,
       [Required] int Quantity,
       [Required] decimal Price
   );

    public class OrderItemResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice
        { get; set; }



        public OrderItemResponse()
        {
          
        }




    }

}
