using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events.Cart
{
    public record CartConvertedToOrderEvent(int CartId, int OrderId) : IDomainEvent;
}
