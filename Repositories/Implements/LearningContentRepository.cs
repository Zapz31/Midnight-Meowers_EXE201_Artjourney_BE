using BusinessObjects.Enums;
using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.ChallengeItem;
using Helpers.DTOs.LearningContent;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public class LearningContentRepository : ILearningContentRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public LearningContentRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public async Task<LearningContent> CreateLearningContentAsync(LearningContent learningContent)
        {
            var createdLearningContent = await _unitOfWork.GetRepo<LearningContent>().CreateAsync(learningContent);
            await _unitOfWork.SaveChangesAsync();
            return createdLearningContent;
        }

        public async Task CreateLearningContentsAsync(List<LearningContent> learningContents)
        {
            await _unitOfWork.GetRepo<LearningContent>().CreateAllAsync(learningContents);
        }

        public async Task<ChallengeItem> CreateChallengeItemAsync(ChallengeItem challengeItem)
        {
            var createdChallengeItem = await _unitOfWork.GetRepo<ChallengeItem>().CreateAsync(challengeItem);
            await _unitOfWork.SaveChangesAsync();
            return createdChallengeItem;
        }

        public async Task<bool> CreateAllChallengeItemAsync(List<ChallengeItem> challengeItems)
        {
            await _unitOfWork.GetRepo<ChallengeItem>().CreateAllAsync(challengeItems);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<long>> GetLearningContentContentIdsByCourseIdAsync(long coursId)
        {
            var options = new QueryBuilder<LearningContent>()
                .WithTracking(false)
                .WithPredicate(lc => lc.CourseId == coursId && lc.IsActive == true)
                .Build();
            var queryData = await _unitOfWork.GetRepo<LearningContent>().GetAllAsync(options);
            var learningContentIdsByCourseIds = queryData.Select(queryData => queryData.LearningContentId);
            return learningContentIdsByCourseIds.ToList();
        }

        public async Task<IEnumerable<LearningContent>> GetLearningContentsBySubmoduleIds(List<long> subModuleIds)
        {
            var learningContentQuery = new QueryBuilder<LearningContent>()
                .WithPredicate(lc => subModuleIds.Contains(lc.SubModuleId) && lc.IsActive == true)
                .WithOrderBy(query => query.OrderBy(lc => lc.DisplayOrder))
                .Build();

            var learningContents = await _unitOfWork.GetRepo<LearningContent>().GetAllAsync(learningContentQuery);
            return learningContents;
        }

        public async Task<List<BasicLearningContentGetResponseDTO>> GetLearningContentsBySubModuleIdAsync(long subModuleId)
        {
            var sql = @"
                SELECT * 
                FROM learning_contents 
                WHERE sub_module_id = {0} AND is_active = true 
                ORDER BY display_order ASC";

            var learningContents =  await _context.LearningContents
                .FromSqlRaw(sql, subModuleId)
                .ToListAsync();

            var dtos = learningContents.Select(l => new BasicLearningContentGetResponseDTO 
            {
                LearningContentId = l.LearningContentId,
                ContentType = l.ContentType,
                ChallengeType = l.ChallengeType,
                Title = l.Title,
                Content = l.Content,
                CorrectAnswer = l.CorrectAnswer,
                TimeLimit = l.TimeLimit,
                CompleteCriteria = l.CompleteCriteria,
                DisplayOrder = l.DisplayOrder,
                LikesCount = l.LikesCount,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                CreatedBy = l.CreatedBy,
                SubModuleId = l.SubModuleId,
                CourseId = l.CourseId
            }).ToList();
            return dtos;
        }

        public async Task<List<BasicChallengeItemGetResponseDTO>> GetChallengeItemsByLNCId(long learingContentId)
        {
            var sql = @"
                        select * from challenge_items ci
                        where ci.learning_content_id = {0}
                        order by ci.item_order";

            var challengeItems = await _context.ChallengeItems
                .FromSqlRaw(sql, learingContentId)
                .ToListAsync();

            var dtos = challengeItems.Select(c => new BasicChallengeItemGetResponseDTO{
                UserId = c.UserId,
                ItemTypes = c.ItemTypes,
                ItemContent = c.ItemContent,
                ItemOrder =  c.ItemOrder,
                Hint = c.Hint,
                AdditionalData = c.AdditionalData,
                LearningContentId = c.LearningContentId
            }).ToList();
            return dtos;

        }

    }
}
