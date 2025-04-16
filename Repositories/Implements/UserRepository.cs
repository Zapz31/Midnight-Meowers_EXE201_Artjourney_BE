using BusinessObjects.Models;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserRepository(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<User> CreateUserAsync(User user)
        {
            var createdUser = await _unitOfWork.GetRepo<User>().CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return createdUser;
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            var queryOptions = new QueryBuilder<User>()
                .WithTracking(false)
                .WithPredicate(u => u.Email.Equals(email))
                .Build();
            var foundUser = _unitOfWork.GetRepo<User>().GetSingleAsync(queryOptions);
            return foundUser;
        }
    }
}
