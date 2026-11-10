using System.Collections.Concurrent;
using Orders.Domain.Model;

namespace Orders.Domain.Ordering;

public interface IOrderRepository
{
    void Save(Order order);

    Order? Find(Guid id);

    IReadOnlyList<Order> All();
}

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public void Save(Order order) => _orders[order.Id] = order;

    public Order? Find(Guid id) => _orders.GetValueOrDefault(id);

    public IReadOnlyList<Order> All() => [.. _orders.Values.OrderBy(o => o.CreatedAt)];
}
