using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class CancellationMustHaveReasonRule : IBusinessRule
    {
        private readonly string _reason;

        public CancellationMustHaveReasonRule(string reason)
        {
            _reason = reason;
        }

        public bool IsBroken() => string.IsNullOrWhiteSpace(_reason);

        public string Message => "Cancellation must have a reason.";
    }
}
