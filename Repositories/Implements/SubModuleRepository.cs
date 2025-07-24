using BusinessObjects.Models;
using DAOs;
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
    public class SubModuleRepository : ISubModuleRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public SubModuleRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<SubModule> CreateUserAsync(SubModule subModule)
        {
            var createdSubModule = await _unitOfWork.GetRepo<SubModule>().CreateAsync(subModule);
            await _unitOfWork.SaveChangesAsync();
            return createdSubModule;
        }

        public async Task<IEnumerable<SubModule>> GetSubModulesByModuleIds(List<long> moduleIds)
        {
            var subModuleQuery = new QueryBuilder<SubModule>()
                .WithPredicate(sm => moduleIds.Contains(sm.ModuleId) && sm.IsActive)
                .WithOrderBy(query => query.OrderBy(sm => sm.DisplayOrder))
                .Build();

            var subModules = await _unitOfWork.GetRepo<SubModule>().GetAllAsync(subModuleQuery);
            return subModules;
        }

        // Get submodules by module ID
        public async Task<List<BasicSubModuleGetResponseDTO>> GetSubModulesByModuleIdAsync(long moduleId)
        {
            var subModules = await _context.SubModules
                .FromSqlRaw("SELECT * FROM sub_modules sm WHERE sm.module_id = {0} and sm.is_active = true ORDER BY display_order ASC", moduleId)
                .ToListAsync();

            var dtos = subModules.Select(sm => new BasicSubModuleGetResponseDTO
            {
                SubModuleId = sm.SubModuleId,
                SubModuleTitle = sm.SubModuleTitle,
                VideoUrls = sm.VideoUrls,
                Description = sm.Description,
                DisplayOrder = sm.DisplayOrder,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                UpdatedAt = sm.UpdatedAt,
                CreatedBy = sm.CreatedBy,
                UpdatedBy = sm.UpdatedBy,
                ModuleId = sm.ModuleId
            }).ToList();

            return dtos;
        }

        public async Task<int> UpdateSubModuleProgress(long userId, long subModuleId, long courseId)
        {

        
            try
            {
                var completeSQL = string.Format(@"DO $$
DECLARE
    total_contents INTEGER;
    completed_contents INTEGER;
    is_sub_module_completed BOOLEAN;
    total_completed_in INTERVAL;
    progress_percentage NUMERIC;
	total_learning_contents INTEGER;
    completed_learning_contents INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_contents
    FROM learning_contents 
    WHERE sub_module_id = {1} AND is_active = true;

    SELECT COUNT(*) INTO completed_contents
    FROM learning_contents lc
    JOIN user_learning_progresses ulp ON lc.learning_content_id = ulp.learning_content_id
    WHERE lc.sub_module_id = {1}
      AND lc.is_active = true
      AND ulp.user_id = {0}
      AND ulp.status = 'Completed';

    SELECT SUM(ulp.completed_in) INTO total_completed_in
    FROM user_learning_progresses ulp
    JOIN learning_contents lc ON ulp.learning_content_id = lc.learning_content_id
    WHERE ulp.user_id = {0}
    AND lc.sub_module_id = {1}
    AND lc.is_active = true
    AND ulp.status = 'Completed';

	SELECT COUNT(*) INTO total_learning_contents
    FROM learning_contents lc
    JOIN sub_modules sm ON lc.sub_module_id = sm.sub_module_id
    JOIN modules m ON sm.module_id = m.module_id
    WHERE m.course_id = {2} AND lc.is_active = true AND sm.is_active = true AND m.deleted_at is null;

    SELECT COUNT(*) INTO completed_learning_contents
    FROM learning_contents lc
    JOIN sub_modules sm ON lc.sub_module_id = sm.sub_module_id
    JOIN modules m ON sm.module_id = m.module_id
    JOIN user_learning_progresses ulp ON lc.learning_content_id = ulp.learning_content_id
    WHERE m.course_id = {2}
      AND lc.is_active = true
	  AND sm.is_active = true
      AND m.deleted_at is null
      AND ulp.user_id = {0}
      AND ulp.status = 'Completed';

    is_sub_module_completed := (total_contents > 0 AND completed_contents = total_contents);
    
	progress_percentage := CASE 
        WHEN total_learning_contents = 0 THEN 0
        ELSE ROUND((completed_learning_contents::NUMERIC / total_learning_contents::NUMERIC) * 100)
    END;

    UPDATE user_sub_module_infos
    SET 
        is_completed = is_sub_module_completed,
        completed_at = CASE 
            WHEN is_sub_module_completed AND completed_at IS NULL 
            THEN NOW() 
            ELSE completed_at 
        END,
        completed_in = CASE 
            WHEN is_sub_module_completed AND completed_in IS NULL 
            THEN total_completed_in
            ELSE completed_in 
        END
    WHERE user_id = {0} AND sub_module_id = {1};

    UPDATE user_course_infos 
    SET 
        progress_percent = progress_percentage
    WHERE user_id = {0} AND course_id = {2};

    IF NOT FOUND THEN
        RAISE EXCEPTION 'No user_sub_module_info record found for user_id: % and sub_module_id: %', {0}, {1};
    END IF;
END $$;", userId, subModuleId, courseId);

                var rowEffect = await _context.Database.ExecuteSqlRawAsync(completeSQL, userId, subModuleId, courseId);
                
                return rowEffect;
            } catch (Exception ex)
            {
               
                throw new Exception($"Error when updating progress for sub_module: {ex.Message}", ex);
            }

        }

        public async Task<int> SoftDeleteSubModuleByIdAsync(long subModuleIdInput)
        {
            try
            {
                // Sử dụng raw query để update (PostgreSQL syntax)
                string sqlQuery = @"
                UPDATE sub_modules 
                SET is_active = @isActive, updated_at = @updatedAt
                WHERE sub_module_id = @subModuleId";

                var parameters = new[]
                {
                new NpgsqlParameter("@subModuleId", subModuleIdInput),
                new NpgsqlParameter("@isActive", false),
                new NpgsqlParameter("@updatedAt", DateTime.UtcNow)
            };

                int result = await _context.Database.ExecuteSqlRawAsync(sqlQuery, parameters);

                return result;
            }
            catch (Exception ex)
            {
                // Log exception nếu cần
                throw new Exception($"Error updating sub module with ID {subModuleIdInput}: {ex.Message}", ex);
            }
        }

    }
}
