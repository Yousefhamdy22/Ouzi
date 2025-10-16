using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }
        [MaxLength(100)]
        public string? LastName { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int? CartId { get; set; }

        // Navigation properties
        public  ICollection<Order> Orders { get; set; }
        public  Cart? Cart { get; set; }
        public  ICollection<Address> Addresses { get; set; }
    }
}
