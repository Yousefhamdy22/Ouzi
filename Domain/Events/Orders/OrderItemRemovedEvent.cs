using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events.Orders
{
    public record OrderItemRemovedEvent(int OrderId, int ProductId, int Quantity, DateTime RemovedDate) : IDomainEvent;
}
