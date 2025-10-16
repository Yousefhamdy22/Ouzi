using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events.Orders
{
    public record OrderCancelledEvent(int OrderId, OrderStatus PreviousStatus, string Reason, DateTime CancelledDate) : IDomainEvent;
}
