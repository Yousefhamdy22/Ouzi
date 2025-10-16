using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events.Orders
{
    public record OrderFailedEvent(int id , string reason) : IDomainEvent;
}
