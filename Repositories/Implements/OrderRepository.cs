using BusinessObjects.Models;
using DAOs;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public OrderRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            var createdOrder = await _unitOfWork.GetRepo<Order>().CreateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return createdOrder;
        }

        public async Task<Order?> GetOrderByUserIdAndOrderCodeSingleAsync(long userId, long orderCode)
        {
            var queryOptions = new QueryBuilder<Order>()
                .WithTracking(false)
                .WithPredicate(o => o.OrderCode == orderCode && o.UserId == userId)
                .Build();

            var data = await _unitOfWork.GetRepo<Order>().GetSingleAsync(queryOptions);
            return data;
        }

        public async Task<Order?> GetOrderByOrderCodeAsync(long orderCode)
        {
            var queryOptions = new QueryBuilder<Order>()
                .WithTracking(false)
                .WithPredicate(o => o.OrderCode == orderCode)
                .Build();
            var data = await _unitOfWork.GetRepo<Order>().GetSingleAsync(queryOptions);
            return data;
        }

        public async Task<bool> UpdateUserPremiumInfoIdAsync(long orderCode, long userPremiumInfoId)
        {
            var sql = @"
            UPDATE orders
            SET user_premium_info_id = {0}
            WHERE order_code = {1};
        ";

            int rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, userPremiumInfoId, orderCode);
            return rowsAffected > 0;
        }
    }
}
