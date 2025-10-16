using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class ShippingMustHaveTrackingNumberRule : IBusinessRule
    {
        private readonly string _trackingNumber;

        public ShippingMustHaveTrackingNumberRule(string trackingNumber)
        {
            _trackingNumber = trackingNumber;
        }

        public bool IsBroken() => string.IsNullOrWhiteSpace(_trackingNumber);

        public string Message => "Shipping must have a tracking number.";
    }
}
