using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserAnswerRepository
    {
        public Task CreateUserAnswers(List<UserAnswer> userAnswers);
        public Task<decimal> CalculateTotalScoreAsync(long quizAttemptId);
    }
}
