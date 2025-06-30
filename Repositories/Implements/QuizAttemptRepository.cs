using BusinessObjects.Models;
using DAOs;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public QuizAttemptRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<QuizAttempt> CreateQuizAttempt(QuizAttempt quizAttempt)
        {
            var createdQuizAttempt = await _unitOfWork.GetRepo<QuizAttempt>().CreateAsync(quizAttempt);
            await _unitOfWork.SaveChangesAsync();
            return createdQuizAttempt;
        }

        public async Task<int> UpdateQuizAttemptWithSubmitQuiz(long quizAttemptId, decimal totalScore)
        {
            var result = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                update quiz_attempts
                set completed_at = now(), 
                total_score = {totalScore},
                is_completed = true,
                time_taken = now() - started_at
                where id = {quizAttemptId}"
             );
            return result;
        }
    }
}
