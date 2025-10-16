using Domain.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class OrderMustHaveItemsRule : IBusinessRule
    {
        private readonly IEnumerable<OrderItem> _items;

        public OrderMustHaveItemsRule(IEnumerable<OrderItem> items)
        {
            _items = items;
        }

        public bool IsBroken() => !_items.Any();
        public string Message => "Order must have at least one item";
    }
}
