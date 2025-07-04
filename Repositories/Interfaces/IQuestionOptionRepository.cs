﻿using BusinessObjects.Models;
using Helpers.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IQuestionOptionRepository
    {
        public Task CreateQuestionOptionsAsync(List<QuestionOptions> questionOptions);
        
    }
}
