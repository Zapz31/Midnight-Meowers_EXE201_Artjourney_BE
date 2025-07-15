using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IOrderItemRepository
    {
        public Task CreateItems(List<OrderItem> orderItems);
        public Task<OrderItem?> GetFirstItemByOrderCode(long orderCode);
    }
}
