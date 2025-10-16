using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class CannotModifyOrderInFinalizedStatusRule : IBusinessRule
    {
        private readonly OrderStatus _status;

        public CannotModifyOrderInFinalizedStatusRule(OrderStatus status)
        {
            _status = status;
        }

        public bool IsBroken() => _status is OrderStatus.Confirmed
            or OrderStatus.Delivered
            or OrderStatus.Cancelled;

        public string Message => "Cannot modify order in finalized status";
    }
}
