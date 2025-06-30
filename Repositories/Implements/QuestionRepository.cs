using BusinessObjects.Models;
using DAOs;
using EFCore.BulkExtensions;
using Helpers.DTOs.LearningContent;
using Helpers.DTOs.Question;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Http.Extensions;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IQuestionOptionRepository _questionOptionRepository;
        public QuestionRepository(IUnitOfWork unitOfWork, ApplicationDbContext context, IQuestionOptionRepository questionOptionRepository)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _questionOptionRepository = questionOptionRepository;
        }
        public async Task CreateQuestionsAsync(List<Question> questions)
        {
            await _unitOfWork.GetRepo<Question>().CreateAllAsync(questions);
        }

        public async Task<bool> CreateQuestionsWithOptionsBulkAsync(List<CreateQuestionsAndOptionsBasicRequestDTO> requestDTOs)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var questions = new List<Question>();

                foreach (var dto in requestDTOs)
                {
                    var question = new Question
                    {
                        QuestionText = dto.QuestionText,
                        QuestionType = dto.QuestionType,
                        Points = dto.Points,
                        OrderIndex = dto.OrderIndex,
                        LearningContentId = dto.LearningContentId,
                        // EF Core tự động handle foreign key
                        QuestionOptions = dto.QuestionOptions.Select(optionDto => new QuestionOptions
                        {
                            OptionText = optionDto.OptionText,
                            IsCorrect = optionDto.IsCorrect,
                            OrderIndex = optionDto.OrderIndex
                            // Không cần set QuestionId
                        }).ToList()
                    };

                    questions.Add(question);
                }

                _context.Questions.AddRange(questions);
                await _context.SaveChangesAsync(); // EF Core tự động set tất cả IDs
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                throw ex;
            }
        }

        public async Task<PaginatedResult<GetQuestionQuizDTO>> GetQuestionWithOptionQuizAsync(long learningContentId, int pageNumber, int pageSize)
        {
            var queryOption = new QueryBuilder<Question>()
                .WithTracking(false)
                .WithInclude(q => q.QuestionOptions.Where(qo => qo.IsActive).OrderBy(qo => qo.OrderIndex))
                .WithPredicate(q => q.IsActive && q.LearningContentId == learningContentId)
                .WithOrderBy(q => q.OrderBy(q => q.OrderIndex))
                .Build();
            var questions = _unitOfWork.GetRepo<Question>().Get(queryOption);
            return await Pagination.ApplyPaginationAsync(
                questions,
                pageNumber,
                pageSize,
                q => new GetQuestionQuizDTO
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Points = q.Points,
                        OrderIndex = q.OrderIndex,
                        QuestionOptions = q.QuestionOptions
                            .Where(qo => qo.IsActive)
                            .OrderBy(qo => qo.OrderIndex)
                            .Select(qo => new GetOptionQuizDTO
                                {
                                    QuestionOptionId = qo.QuestionOptionId,
                                    OptionText = qo.OptionText,
                                    OrderIndex = qo.OrderIndex
                            }).ToList()
                     }
                );
        }
    }
}
