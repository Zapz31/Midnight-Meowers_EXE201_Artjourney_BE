using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Module;
using Helpers.DTOs.SubModule;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        public async Task<int> UpdateModuleProgress(long userId, long moduleId)
        {
            try
            {
                
                var completeSQL = string.Format(@"DO $$
DECLARE
    total_sub_modules INTEGER;
    completed_sub_modules INTEGER;
    is_module_completed BOOLEAN;
    total_completed_in INTERVAL;
BEGIN
    SELECT COUNT(*) INTO total_sub_modules
    FROM sub_modules 
    WHERE module_id = {1} AND is_active = true;

SELECT COUNT(*) INTO completed_sub_modules
    FROM sub_modules sm
    JOIN user_sub_module_infos usmi ON sm.sub_module_id = usmi.sub_module_id
    WHERE sm.module_id = {1} 
      AND sm.is_active = true
      AND usmi.user_id = {0} 
      AND usmi.is_completed = true;

is_module_completed := (total_sub_modules > 0 AND completed_sub_modules = total_sub_modules);

	SELECT SUM(usmi.completed_in) INTO total_completed_in
    FROM user_sub_module_infos usmi
    JOIN sub_modules sm ON sm.sub_module_id = usmi.sub_module_id
    WHERE usmi.user_id = {0}
      AND sm.module_id = {1}
      AND sm.is_active = true
      AND usmi.is_completed = true;

    -- update user_module_infos
    UPDATE user_module_infos
    SET 
        is_completed = is_module_completed,
        completed_at = CASE 
            WHEN is_module_completed AND completed_at IS NULL 
            THEN NOW() 
            ELSE completed_at 
        END,
        completed_in = CASE 
            WHEN is_module_completed AND completed_in IS NULL
            THEN total_completed_in
            ELSE completed_in
        END
    WHERE user_id = {0} AND module_id = {1};

    -- Raise exception if no record was updated
    IF NOT FOUND THEN
        RAISE EXCEPTION 'No user_module_info record found for user_id: % and sub_module_id: %', {0}, {1};
    END IF;
END $$;", userId, moduleId);
                var rowEffect = await _context.Database.ExecuteSqlRawAsync(completeSQL, userId, moduleId);
                
                return rowEffect;
            } catch (Exception ex)
            {
                
                throw new Exception($"Error when updating progress for sub_module: {ex.Message}", ex);
            }
        }

        public async Task<int> SoftDeleteModuleByModuleId(long moduleId, long userId)
        {
            try
            {
                string sqlQuery = @"
                    update modules
                    set deleted_at = @deletedAt, deleted_by = @deletedBy
                    where module_id = @moduleId";

                var parameters = new[]
                {
                    new NpgsqlParameter("@deletedAt", DateTime.UtcNow),
                    new NpgsqlParameter("@deletedBy", userId),
                    new NpgsqlParameter("@moduleId", moduleId)
                };
                int result = await _context.Database.ExecuteSqlRawAsync(sqlQuery, parameters);

                return result;
            } catch (Exception ex)
            {
                throw new Exception($"Error updating module with ID {moduleId}: {ex.Message}");
            }
        }
    }
}
