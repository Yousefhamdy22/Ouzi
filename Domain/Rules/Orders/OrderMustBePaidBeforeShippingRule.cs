using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class OrderMustBePaidBeforeShippingRule : IBusinessRule
    {
        private readonly PaymentStatus _paymentStatus;

        public OrderMustBePaidBeforeShippingRule(PaymentStatus paymentStatus)
        {
            _paymentStatus = paymentStatus;
        }

        public bool IsBroken() => _paymentStatus != PaymentStatus.Paid;

        public string Message => "Order must be paid before it can be shipped.";
    }

 
    public class OrderMustBeInProcessingStatusToShipRule : IBusinessRule
    {
        private readonly OrderStatus _status;

        public OrderMustBeInProcessingStatusToShipRule(OrderStatus status)
        {
            _status = status;
        }

        public bool IsBroken() => _status != OrderStatus.Pending;

        public string Message => "Order must be in Processing status to be shipped.";
    }
}
