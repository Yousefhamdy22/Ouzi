using Domain.Common;

namespace Domain.Events.Orders
{
    public record OrderConfirmedEvent(int OrderId) : IDomainEvent;
}
