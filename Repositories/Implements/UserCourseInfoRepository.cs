using DAOs;
using Helpers.DTOs.UserCourseInfo;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserCourseInfoRepository : IUserCourseInfoRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserCourseInfoRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        //public async Task<List<BasicUserCourseInfoGetReponseDTO>> GetBasicUserCourseInfoGetReponseDTOs(long userId, long courseId)
        //{
        //    var sql = @"";
        //}
    }
}
