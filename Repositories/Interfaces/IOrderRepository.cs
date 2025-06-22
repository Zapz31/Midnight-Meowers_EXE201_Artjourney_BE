using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IOrderRepository
    {
        public Task<Order> CreateOrder(Order order);
        public Task<Order?> GetOrderByUserIdAndOrderCodeSingleAsync(long userId, long orderCode);
        public Task<Order?> GetOrderByOrderCodeAsync(long orderCode);
    }
}
