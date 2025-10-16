using Domain.Common;
using Domain.Entities;


namespace Domain.Rules.Orders
{

    public class OrderMustHaveValidAddressRule : IBusinessRule
    {
        private readonly Address _address;

        public OrderMustHaveValidAddressRule(Address address)
        {
            _address = address;
        }

        public bool IsBroken()
        {
         
            if (string.IsNullOrWhiteSpace(_address.Street))
                return true;

            if (string.IsNullOrWhiteSpace(_address.City))
                return true;

            //if (string.IsNullOrWhiteSpace(_address.State))
            //    return true;



            return false;
        }

        public string Message => "Order must have a valid complete address with street, city, state, zip code, and country.";



    }
}

