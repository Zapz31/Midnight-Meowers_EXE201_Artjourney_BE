using BusinessObjects.Enums;
using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Courses;
using Helpers.DTOs.HistoricalPeriod;
using Helpers.HelperClasses;
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
    public class CourseRepository : ICourseRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public CourseRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            var createdCourse = await _unitOfWork.GetRepo<Course>().CreateAsync(course);
            await _unitOfWork.SaveChangesAsync();
            return createdCourse;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            await _unitOfWork.GetRepo<Course>().UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<LearnPageCourseReginDTO>> GetAllPublishedCoursesGroupedByRegionAsync()
        {
            
            var regionHistoricalQuery = from rhp in _unitOfWork.GetRepo<RegionHisoricalPeriod>()
                                            .Get(new QueryOptions<RegionHisoricalPeriod> { Tracked = false })
                                        join r in _unitOfWork.GetRepo<Region>()
                                            .Get(new QueryOptions<Region> { Tracked = false }) on rhp.RegionId equals r.RegionId
                                        join hp in _unitOfWork.GetRepo<HistoricalPeriod>()
                                            .Get(new QueryOptions<HistoricalPeriod> { Tracked = false })
                                            .Include(hp => hp.CreatedUser) on rhp.HistoricalPeriodId equals hp.HistoricalPeriodId
                                        where r.DeletedAt == null && hp.DeletedAt == null
                                        select new
                                        {
                                            RegionId = r.RegionId,
                                            RegionName = r.RegionName,
                                            HistoricalPeriod = hp
                                        };

            var regionHistoricalData = await regionHistoricalQuery.ToListAsync();

            
            var courseQuery = from c in _unitOfWork.GetRepo<Course>()
                                  .Get(new QueryOptions<Course> { Tracked = false })
                              join r in _unitOfWork.GetRepo<Region>()
                                  .Get(new QueryOptions<Region> { Tracked = false }) on c.RegionId equals r.RegionId
                              where r.DeletedAt == null && c.ArchivedAt == null && c.Status == CourseStatus.Published
                              orderby c.EnrollmentCount descending
                              select new
                              {
                                  RegionId = r.RegionId,
                                  Course = c
                              };

            var courseData = await courseQuery.ToListAsync();

            
            var result = regionHistoricalData
                .GroupBy(x => new { x.RegionId, x.RegionName })
                .Select(regionGroup => new LearnPageCourseReginDTO
                {
                    RegionId = regionGroup.Key.RegionId,
                    RegionName = regionGroup.Key.RegionName,
                    historicalPeriodDTOs = regionGroup
                        .Select(x => new HistoricalPeriodDTO
                        {
                            HistoricalPeriodId = x.HistoricalPeriod.HistoricalPeriodId,
                            HistoricalPeriodName = x.HistoricalPeriod.HistoricalPeriodName,
                            Description = x.HistoricalPeriod.Description,
                            StartYear = x.HistoricalPeriod.StartYear,
                            EndYear = x.HistoricalPeriod.EndYear,
                        })
                        .ToList(),
                    Courses = courseData
                        .Where(cd => cd.RegionId == regionGroup.Key.RegionId)
                        .Select(cd => new CourseDTO
                        {
                            CourseId = cd.Course.CourseId,
                            Title = cd.Course.Title,
                            ThumbnailImageUrl = cd.Course.ThumbnailUrl,
                            RegionId = cd.RegionId,
                            HistoricalPeriodId = cd.Course.HistoricalPeriodId
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }
       
        public async Task<PaginatedResult<SearchResultCourseDTO>> SearchCoursesAsync(string? input, int pageNumber = 1, int pageSize = 10)
        {

            IQueryable<Course>? baseQuery = null;
            if (input != null)
            {
                baseQuery = _unitOfWork.GetRepo<Course>().Get(new QueryBuilder<Course>()
                .WithPredicate(c => c.Status == CourseStatus.Published &&
                           c.Title.ToLower().Contains(input.ToLower()))
                .WithOrderBy(q => q.OrderByDescending(c => c.AverageRating))
                .WithTracking(false)
                .Build());
            } else
            {
                baseQuery = _unitOfWork.GetRepo<Course>().Get(new QueryBuilder<Course>()
                .WithPredicate(c => c.Status == CourseStatus.Published)
                .WithOrderBy(q => q.OrderByDescending(c => c.AverageRating))
                .WithTracking(false)
                .Build());
            }

            // Mapping từ Course sang SearchResultCourseDTO
            var paginatedResult = await Pagination.ApplyPaginationAsync(
                baseQuery,
                pageNumber,
                pageSize,
                course => new SearchResultCourseDTO
            {
                CourseId = course.CourseId,
                Title = course.Title,
                ThumbnailUrl = course.ThumbnailUrl,
                AverageRating = course.AverageRating,
                TotalFeedbacks = course.TotalFeedbacks,
                Level = course.Level,
                Price = course.Price,
            });

            return paginatedResult;
        }

        public async Task<Course?> GetSingleCourseAsync(long courseId)
        {
            var option = new QueryBuilder<Course>()
                .WithTracking(false)
                .WithPredicate(c => c.CourseId == courseId && c.Status == CourseStatus.Published)
                .Build();
            return await _unitOfWork.GetRepo<Course>().GetSingleAsync(option);
        }

        public Task<bool> UpdateTotalFeedBackAndAverageRatingAsync(long courseId, int newTotalFeeback, decimal newAverageRating)
        {
            throw new NotImplementedException();
        }

        public async Task<CourseLearningStatisticsDTO> GetCourseLearningStatisticsOptimizedAsync(long courseId)
        {
            var moduleRepo = _unitOfWork.GetRepo<Module>();
            var subModuleRepo = _unitOfWork.GetRepo<SubModule>();
            var learningContentRepo = _unitOfWork.GetRepo<LearningContent>();

            // Tạo query với điều kiện filter giống SQL
            var moduleQuery = moduleRepo.Get(new QueryOptions<Module>
            {
                Predicate = m => m.CourseId == courseId && m.DeletedAt == null,
                Tracked = false
            });

            var subModuleQuery = subModuleRepo.Get(new QueryOptions<SubModule>
            {
                Predicate = sm => sm.IsActive == true,
                Tracked = false
            });

            var learningContentQuery = learningContentRepo.Get(new QueryOptions<LearningContent>
            {
                
                Predicate = lc => lc.IsActive == true, 
                Tracked = false
            });

            var joinedQuery = from m in moduleQuery
                              join sm in subModuleQuery on m.ModuleId equals sm.ModuleId into smGroup
                              from sm in smGroup.DefaultIfEmpty()
                              join lc in learningContentQuery on sm != null ? sm.SubModuleId : 0 equals lc.SubModuleId into lcGroup
                              from lc in lcGroup.DefaultIfEmpty()
                              select new
                              {
                                  TimeLimit = lc != null ? lc.TimeLimit : null,
                                  LearningContentId = lc != null ? lc.LearningContentId : (long?)null
                              };

            var results = await joinedQuery.ToListAsync();

            var totalLearningTime = results
                .Where(r => r.TimeLimit.HasValue)
                .Sum(r => r.TimeLimit?.Ticks ?? 0);

            var totalLearningContent = results
                .Count(r => r.LearningContentId.HasValue);

            return new CourseLearningStatisticsDTO
            {
                TotalLearningTime = new TimeSpan(totalLearningTime),
                TotalLearningContent = totalLearningContent
            };
        }

        public async Task<List<CourseDetailScreenFlat>> GetCourseDetailScreenFlatAsync(long courseId)
        {
            var sql = @"
                select m.module_id, 
                m.module_title, 
                sm.sub_module_id, 
                sm.sub_module_title, 
                lc.learning_content_id, 
                lc.title, 
                lc.display_order, 
                lc.time_limit from modules m 
                left join sub_modules sm on m.module_id = sm.module_id
                left join learning_contents lc on sm.sub_module_id = lc.sub_module_id
                where m.course_id = {0} and m.deleted_at is null and sm.is_active = true and lc.is_active = true";
            var data = await _context.Set<CourseDetailScreenFlat>()
                .FromSqlRaw(sql, courseId)
                .ToListAsync();
            return data;
        }

    }
}
