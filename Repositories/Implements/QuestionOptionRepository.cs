using BusinessObjects.Models;
using DAOs;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class QuestionOptionRepository : IQuestionOptionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public QuestionOptionRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public async Task CreateQuestionOptionsAsync(List<QuestionOptions> questionOptions)
        {
            await _unitOfWork.GetRepo<QuestionOptions>().CreateAllAsync(questionOptions);
        }
    }
}
