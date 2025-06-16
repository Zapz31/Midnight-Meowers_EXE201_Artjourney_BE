using BusinessObjects.Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserSubModuleInfoRepository : IUserSubModuleInfoRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserSubModuleInfoRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateUserSubModules(List<UserSubModuleInfo> userSubModuleInfos)
        {
            await _unitOfWork.GetRepo<UserSubModuleInfo>().CreateAllAsync(userSubModuleInfos);
        }

    }
}
