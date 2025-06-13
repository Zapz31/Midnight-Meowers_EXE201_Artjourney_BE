using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Module;
using Helpers.DTOs.SubModule;
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
    public class ModuleRepository : IModuleRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public ModuleRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<Module> CreateModuleAsync(Module module)
        {
            var createdModule = await _unitOfWork.GetRepo<Module>().CreateAsync(module);
            await _unitOfWork.SaveChangesAsync();
            return createdModule;
        }

        public async Task<IEnumerable<Module>> GetModulesByCourseId(long courseId)
        {
            var moduleQuery = new QueryBuilder<Module>()
                .WithPredicate(m => m.CourseId == courseId && m.DeletedAt == null)
                .WithOrderBy(query => query.OrderBy(m => m.ModuleId))
                .Build();

            var modules = await _unitOfWork.GetRepo<Module>().GetAllAsync(moduleQuery);
            return modules;
        }

        public async Task<List<BasicModuleGetResponseDTO>> GetModulesByCourseIdCompletedAsync(long courseId)
        {
            var modules = await _context.Modules
                .FromSqlRaw("select * from modules m where m.course_id = {0} and m.deleted_at is null", courseId)
                .ToListAsync();

            var dtos = modules.Select(m => new BasicModuleGetResponseDTO
            {
                ModuleId = m.ModuleId,
                ModuleTitle = m.ModuleTitle,
                Description = m.Description,
                UpdatedBy = m.UpdatedBy,
                DeletedBy = m.DeletedBy,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                DeletedAt = m.DeletedAt,
                CourseId = m.CourseId,
                CreatedBy = m.CreatedBy,
                
            }).ToList();

            return dtos;
        }


    }
}
