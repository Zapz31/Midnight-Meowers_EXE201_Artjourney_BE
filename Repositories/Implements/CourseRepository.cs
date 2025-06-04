using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Courses;
using Helpers.DTOs.HistoricalPeriod;
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
        public CourseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            var createdCourse = await _unitOfWork.GetRepo<Course>().CreateAsync(course);
            await _unitOfWork.SaveChangesAsync();
            return createdCourse;
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
                            ThumbnailImageUrl = cd.Course.ThumbnailUrl
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }
    }
}
