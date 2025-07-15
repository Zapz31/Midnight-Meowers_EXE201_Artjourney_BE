using BusinessObjects.Models;
using DAOs;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public OrderItemRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task CreateItems(List<OrderItem> orderItems)
        {
            await _unitOfWork.GetRepo<OrderItem>().CreateAllAsync(orderItems);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<OrderItem?> GetFirstItemByOrderCode(long orderCode)
        {
            return await _context.OrderItems
            .Where(oi => oi.OrderCode == orderCode)
            .FirstOrDefaultAsync();
        }
    }
}
