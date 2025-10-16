
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entities
{
    public class Address
    {
    
        public int Id { get; set; }


        
        [ForeignKey("User")]
        public string UserId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }


        public ApplicationUser User { get; set; }

        public Address()
        {

        }

        public Address(string street, string city)
        {
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street is required");
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required");
        

            Street = street;
            City = city;
          
        }

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    yield return Street.ToLowerInvariant();
        //    yield return City.ToLowerInvariant();
        //    yield return State.ToLowerInvariant();
         
        //}
    }
}
