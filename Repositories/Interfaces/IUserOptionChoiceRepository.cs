using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserOptionChoiceRepository
    {
        Task<List<UserOptionChoice>> GetUserChoicesByUserIdAsync(long userId);
        Task<UserOptionChoice?> GetUserChoiceByUserAndOptionAsync(long userId, long surveyOptionId);
        Task<UserOptionChoice> CreateUserOptionChoiceAsync(UserOptionChoice userOptionChoice);
        Task UpdateUserOptionChoiceAsync(UserOptionChoice userOptionChoice);
        Task DeleteUserOptionChoiceAsync(long userOptionChoiceId);
        Task<List<UserOptionChoice>> CreateMultipleUserOptionChoicesAsync(List<UserOptionChoice> userOptionChoices);
        Task<bool> HasUserCompletedSurveyAsync(long userId);
        Task DeleteAllUserChoicesAsync(long userId);
    }
}
