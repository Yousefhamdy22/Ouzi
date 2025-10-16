using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class CannotRemoveItemFromFinalizedOrderRule : IBusinessRule
    {
        private readonly OrderStatus _status;

        public CannotRemoveItemFromFinalizedOrderRule(OrderStatus status)
        {
            _status = status;
        }

        public bool IsBroken() => _status == OrderStatus.Confirmed ||
                                 _status == OrderStatus.Delivered ||
                                 _status == OrderStatus.Cancelled;

        public string Message => "Cannot remove items from shipped, delivered, or cancelled orders.";
    }

    // Domain/Rules/OrderMustHaveAtLeastOneItemRule.cs
    public class OrderMustHaveAtLeastOneItemRule : IBusinessRule
    {
        private readonly int _itemsCount;

        public OrderMustHaveAtLeastOneItemRule(int itemsCount)
        {
            _itemsCount = itemsCount;
        }

        public bool IsBroken() => _itemsCount == 0;

        public string Message => "Order must have at least one item.";
    }
}
