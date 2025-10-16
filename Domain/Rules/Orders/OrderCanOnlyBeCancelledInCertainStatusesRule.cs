using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class OrderCanOnlyBeCancelledInCertainStatusesRule : IBusinessRule
    {
        private readonly OrderStatus _status;

        public OrderCanOnlyBeCancelledInCertainStatusesRule(OrderStatus status)
        {
            _status = status;
        }

        public bool IsBroken()
        {
          
            return _status == OrderStatus.Delivered ||
                   _status == OrderStatus.Confirmed || 
                   _status == OrderStatus.Cancelled;
        }

        public string Message => "Order can only be cancelled in Pending, Confirmed, or Processing status.";
    }

  
    public class CannotCancelPaidOrderWithoutRefundRule : IBusinessRule
    {
        private readonly PaymentStatus _paymentStatus;

        public CannotCancelPaidOrderWithoutRefundRule(PaymentStatus paymentStatus)
        {
            _paymentStatus = paymentStatus;
        }

        public bool IsBroken() => false; 

        public string Message => "Paid order cancellation requires refund processing.";
    }
}
