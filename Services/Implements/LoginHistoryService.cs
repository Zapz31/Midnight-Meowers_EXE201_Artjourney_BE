using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.HelperClasses;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        public LoginHistoryService(ILoginHistoryRepository loginHistoryRepository)
        {
            _loginHistoryRepository = loginHistoryRepository;
        }
        public async Task<ApiResponse<LoginHistory?>> CreateLoginHistoryAsync(LoginHistory loginHistory)
        {
            try
            {
                LoginHistory createdLoginHistory = await _loginHistoryRepository.CreateLoginHistory(loginHistory);
                ApiResponse<LoginHistory?> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 201,
                    Data = loginHistory,
                    Message = "2003"
                };
                return response;
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                ApiResponse<LoginHistory?> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors =
                    [
                        new ApiError{ Code = 1010}
                    ]
                };
                return response;
            }
        }

        public async Task<ApiResponse<long>> GetMaxLoginHistoryIdAsync()
        {
            try
            {
                long maxId = await _loginHistoryRepository.GetMaxLoginHistoryIdAsync();
                ApiResponse<long> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = maxId,

                };
                return response;
            } catch(Exception ex) 
            {
                Console.WriteLine(ex);
                ApiResponse<long> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors =
                    [
                        new ApiError{Code = 1011}
                    ]
                };
                return response;
            }
            
        }
    }
}
