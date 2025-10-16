using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class OrderMustHaveUserIdRule : IBusinessRule
    {
        private readonly Guid _userId;
        public OrderMustHaveUserIdRule(Guid userId) => _userId = userId;
        public bool IsBroken() => string.IsNullOrWhiteSpace(_userId.ToString());
        public string Message => "Order must have a user ID.";
    }
}
