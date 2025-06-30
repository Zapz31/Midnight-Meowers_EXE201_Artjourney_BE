using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.General;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserAnswerRepository : IUserAnswerRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserAnswerRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task CreateUserAnswers(List<UserAnswer> userAnswers)
        {
            await _unitOfWork.GetRepo<UserAnswer>().CreateAllAsync(userAnswers);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> CalculateTotalScoreAsync(long quizAttemptId)
        {
            var sql = @"with quiz_info as (
  select 
    qa.id as quiz_attempt_id,
    qa.learning_content_id
  from quiz_attempts qa
  where qa.id = @quizAttemptId
),

correct_options as (
  select 
    q.question_id,
    q.question_type,
    q.points,
    qo.question_option_id
  from questions q
  join question_options qo on q.question_id = qo.question_id
  join quiz_info qi on q.learning_content_id = qi.learning_content_id
  where q.is_active = true and qo.is_active = true and qo.is_correct = true
),

incorrect_options as (
  select 
    q.question_id,
    qo.question_option_id
  from questions q
  join question_options qo on q.question_id = qo.question_id
  join quiz_info qi on q.learning_content_id = qi.learning_content_id
  where q.is_active = true and qo.is_active = true and qo.is_correct = false
),

user_answers_filtered as (
  select 
    ua.quiz_attempt_id,
    ua.question_id,
    ua.selected_option_id
  from user_answers ua
  join quiz_info qi on ua.quiz_attempt_id = qi.quiz_attempt_id
),

questions_info as (
  select distinct q.question_id, q.question_type, q.points
  from questions q
  join quiz_info qi on q.learning_content_id = qi.learning_content_id
  where q.is_active = true
),

quiz_questions as (
  select 
    qi.question_id, 
    qi.question_type, 
    qi.points, 
    qi2.quiz_attempt_id
  from questions_info qi
  join quiz_info qi2 on true  -- để lấy quiz_attempt_id từ quiz_info
),

scored as (
  select 
    qq.quiz_attempt_id,
    qq.question_id,
    qq.question_type,
    qq.points,

    -- Tổng số đáp án đúng của câu hỏi
    (select count(*) from correct_options co where co.question_id = qq.question_id) as total_correct,

    -- Số đáp án đúng mà user đã chọn
    (select count(*) 
     from correct_options co 
     join user_answers_filtered uaf2 
       on co.question_id = uaf2.question_id and co.question_option_id = uaf2.selected_option_id
     where co.question_id = qq.question_id and uaf2.quiz_attempt_id = qq.quiz_attempt_id
    ) as user_selected_correct,

    -- Số đáp án sai mà user đã chọn
    (select count(*) 
     from incorrect_options io
     join user_answers_filtered uaf3 
       on io.question_id = uaf3.question_id and io.question_option_id = uaf3.selected_option_id
     where io.question_id = qq.question_id and uaf3.quiz_attempt_id = qq.quiz_attempt_id
    ) as user_selected_incorrect

  from quiz_questions qq
)
select sum(t.awarded_points) as total_score from(
select 
  case
    when question_type = 'SingleChoice' and user_selected_correct = 1 then points
    when question_type = 'MultipleChoice' 
         and user_selected_correct = total_correct 
         and user_selected_incorrect = 0 then points
    else 0
  end as awarded_points
from scored
) as t";

            var param = new Npgsql.NpgsqlParameter("@quizAttemptId", quizAttemptId);

            var result = await _context
                .Database
                .ExecuteSqlRawAsync("SET TIME ZONE 'UTC';"); // nếu muốn đảm bảo đồng bộ timezone

            var totalScore = await _context
                .Set<TotalScoreResult>()
                .FromSqlRaw(sql, param)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return totalScore?.TotalScore ?? 0m;
        }
    }
}
