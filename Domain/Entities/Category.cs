using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category
    {
       
        public int Id { get; set; }

        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

       
        public  ICollection<Product> Products { get; set; }
    }
}
