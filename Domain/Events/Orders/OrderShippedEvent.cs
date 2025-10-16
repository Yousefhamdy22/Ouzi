using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events.Orders
{
    public record OrderShippedEvent(int OrderId, string TrackingNumber, DateTime ShippedDate) : IDomainEvent;
}
