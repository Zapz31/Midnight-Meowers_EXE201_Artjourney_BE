using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserCourseInfoRepository
    {
        public Task<UserCourseInfo> CreateUserCourseInfo(UserCourseInfo userCourseInfo);
        public Task<List<UserCourseInfo>> GetUserCourseInfosByUserIdAndCourseId(long userId, long courseId);
    }
}
