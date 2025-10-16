using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AddressDto
    {

        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }

        public bool IsDefault { get; set; }

    }
}
