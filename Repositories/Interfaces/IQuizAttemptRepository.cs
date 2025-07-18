﻿using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IQuizAttemptRepository
    {
        public Task<QuizAttempt> CreateQuizAttempt(QuizAttempt quizAttempt);
        public Task<int> UpdateQuizAttemptWithSubmitQuiz(long quizAttemptId, decimal totalScore);
    }
}
