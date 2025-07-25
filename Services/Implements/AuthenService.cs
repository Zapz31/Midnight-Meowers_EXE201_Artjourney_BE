﻿using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.Users;
using Helpers.HelperClasses;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UUIDNext;

namespace Services.Implements
{
    public class AuthenService : IAuthenService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly IVerificationInfoRepository _verificationInfoRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMailSenderService _mailSender;
        private readonly ILogger<AuthenService> _logger;
        private readonly IUserPremiumInfoRepository _userPremiumInfoRepository;
        public AuthenService(IUserService userService,
            ITokenService tokenService,
            ILoginHistoryService loginHistoryService,
            IVerificationInfoRepository verificationInfoRepository,
            ICurrentUserService currentUserService, 
            IMailSenderService mailSender,
            ILogger<AuthenService> logger,
            IUserPremiumInfoRepository userPremiumInfoRepository
            )
        {
            _userService = userService;
            _tokenService = tokenService;
            _loginHistoryService = loginHistoryService;
            _verificationInfoRepository = verificationInfoRepository;
            _currentUserService = currentUserService;
            _mailSender = mailSender;
            _logger = logger;
            _currentUserService = currentUserService;
            _userPremiumInfoRepository = userPremiumInfoRepository;
        }
        public async Task<AuthenticationResponse?> Register(RegisterDTO registerDto)
        {
            var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return null;
            }
            var createdUser = await _userService.CreateAccount(registerDto);
            return new AuthenticationResponse()
            {
                Token = _tokenService.CreateVerifyToken(createdUser)
            };
        }

        public async Task<ApiResponse<AuthenticationResponse>> Login(LoginDTO loginDto, string ipAddress, string? userAgent)
        {
            var account = await _userService.GetUserByEmailAsync(loginDto.Email);

            if (account == null)
            {
                return new ApiResponse<AuthenticationResponse>()
                {
                    Status = ResponseStatus.Error,
                    Code = 404,
                    Errors =
                    [
                        new ApiError{Code = 1007}
                    ]
                };
            }
                

            bool isPasswordValid = PasswordHasher.VerifyPassword(
                loginDto.Password,
                account.Password
            );

            if (!isPasswordValid)
            {
                return new ApiResponse<AuthenticationResponse>()
                {
                    Status = ResponseStatus.Error,
                    Code = 404,
                    Errors =
                    [
                        new ApiError{Code = 1007}
                    ]
                };
            }
            if (account.Status == AccountStatus.Banned)
            {
                return new ApiResponse<AuthenticationResponse>()
                {
                    Status = ResponseStatus.Error,
                    Code = 404,
                    Errors =
                    [
                        new ApiError{Code = 1008}
                    ]
                };
            }
            ApiResponse<long> maxLoginHistoryId = await _loginHistoryService.GetMaxLoginHistoryIdAsync();
            LoginHistory loginHistory = new LoginHistory()
            {
                UserId = account.UserId,
                LoginHistoryId = maxLoginHistoryId.Data + 1,
                LoginTimestamp = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                LoginResult = LoginResult.Success,
            };

            await _loginHistoryService.CreateLoginHistoryAsync( loginHistory );

            string jwt = _tokenService.CreateToken(account);
            NewUpdateUserDTO user = new(account);
            AuthenticationResponse data = new AuthenticationResponse()
            {
                Token = jwt,
                UserDTO = user
            };


            return new ApiResponse<AuthenticationResponse>()
            { 
                Status = ResponseStatus.Success,
                Code = 200,
                Data = data,
                Message = "2001"
            };
        }

        public async Task<ApiResponse<NewUpdateUserDTO?>> getUserByIdAuthAsync(long userId)
        {
            ApiResponse<NewUpdateUserDTO?> response = await _userService.GetUserByIDAsynce(userId);
            return response;
        }

        public async Task<ApiResponse<User>> CreateOrUpdateUserByEmailAsync(string email, string? name, string? avatar)
        {
            try
            {
                _logger.LogInformation("Đã vào hàm CreateOrUpdateUserByEmailAsync với Email={Email}, name = {Name}, avatar = {Avatar}", email, name, avatar);
                ApiResponse<User> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                };
                _logger.LogInformation("1. Go to GetUserByEmailAsync function with parameter email: email={Email}", email);
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogInformation("1.1.1 Sau khi thực hiện call _userService.GetUserByEmailAsync() thì user = null");
                    user = new User
                    {
                        Email = email,
                        Fullname = name ?? "",
                        AvatarUrl = avatar ?? ""
                    };
                    _logger.LogInformation("1.1.2 Thực hiện tạo user mới chưa có trong hệ thống _userService.CreateCompleteAccount");
                    response.Data = await _userService.CreateCompleteAccount(user); // B13: Tạo mới người dùng
                    if (response.Data  == null)
                    {
                        _logger.LogError("Hàm _userService.CreateCompleteAccount(user) sau khi tạo user thì không trả về user này");
                    }
                }
                else
                {
                    _logger.LogInformation("1.2. Đã tìm thấy user ở (1) trong hàm _userService.GetUserByEmailAsync(email) với email={Email}", email);
                    _logger.LogInformation("1.2.1 Kiểm tra tài khoản này đã được tạo trên hệ thống trước đó hay chưa");
                    // Check if this email has already registered by another user
                    if (!string.IsNullOrWhiteSpace(user.Password))
                    {
                        _logger.LogError("Email này đã được tạo bằng system account trước đó, email={Email}", email);
                        return new ApiResponse<User>
                        {
                            Status = ResponseStatus.Error,
                            Code = 404,
                            Errors =
                            [
                                new ApiError {Code = 1006}
                            ]
                        };
                    }
                    
                    NewUpdateUserDTO newUpdateUserDTO = new NewUpdateUserDTO
                    {
                        Email = email,
                        FullName = name,
                        AvatarUrl = avatar
                    };
                    _logger.LogInformation("1.2.2 gg account đã được tạo trước đó, tiến hành cập nhật thông tin nếu có thay đổi");
                    ApiResponse<User> updatedUserResponse = await _userService.UpdateUserAsync(newUpdateUserDTO);
                    if (updatedUserResponse.Status.Equals(ResponseStatus.Error))
                    {
                        _logger.LogInformation("1.2.3 Error trong quá trình _userService.UpdateUserAsync(newUpdateUserDTO)");
                        return updatedUserResponse;
                    }
                    if (updatedUserResponse.Data == null)
                    {
                        _logger.LogInformation("_userService.UpdateUserAsync(newUpdateUserDTO); thành công nhưng không trả về đối tượng User (updatedUserResponse.Data == null)");
                    }
                    response.Data = updatedUserResponse.Data;
                }

                if (response.Data == null)
                {
                    _logger.LogError("Cập nhật hoặc tạo user thành công nhưng không trả về (Login bằng gmail)");
                    response.Status = ResponseStatus.Error;
                    response.Code = 500;
                    response.Errors =
                        [
                            new ApiError {Code = 1005}
                        ];

                    return response;
                }

                _logger.LogInformation("Thực hiện _loginHistoryService.GetMaxLoginHistoryIdAsync();");
                ApiResponse<long> maxLoginHistoryId = await _loginHistoryService.GetMaxLoginHistoryIdAsync();
                LoginHistory loginHistory = new LoginHistory()
                {
                    UserId = response.Data.UserId,
                    LoginHistoryId = maxLoginHistoryId.Data + 1,
                    LoginTimestamp = DateTime.UtcNow,                   
                    LoginResult = LoginResult.Success,
                };
                _logger.LogInformation("Thực hiện _loginHistoryService.CreateLoginHistoryAsync(loginHistory)");
                await _loginHistoryService.CreateLoginHistoryAsync(loginHistory);

                response.Message = _tokenService.CreateToken(response.Data);

                _logger.LogInformation("Chuyển hướng lên contronller");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateOrUpdateUserByEmailAsync has 500 error: {Exmess}", ex.Message);
                return new ApiResponse<User> 
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors = 
                    [
                        new ApiError {Code = 1004}
                    ]
                };
            }  
        }
        public async Task<ApiResponse<string>> SendVerificationEmail()
        {
            try
            {
                var userId = _currentUserService.AccountId; 
                var userData = await _userService.GetUserByIDAsynce(userId);
                if (userData.Data == null) {
                    ApiResponse<string> errorStatusResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Data = null,
                        Errors =
                            [
                                new ApiError{Code = 1009}
                            ]
                    };
                }
                //check if users has verify 
                if (userData.Data.Status != AccountStatus.Pending)
                {
                    ApiResponse<string> errorStatusResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 400,
                        Data = null,
                        Errors = 
                            [
                                new ApiError{Code = 1013}
                            ]
                    };
                    return errorStatusResponse;
                }
                //delete all verification record before
                var deletedRecord = await _verificationInfoRepository.DeleteVerificationInfosByEmail(userData.Data.Email);

                

                //generate token and expire time
                var token = Uuid.NewRandom().ToString();
                var expiresAt = DateTime.UtcNow.AddMinutes(17);
                VerificationInfo verificationInfo = new()
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    Email = userData.Data.Email,
                };

                // inseart verificationInfo to db
                var createdVerificationInfo = await _verificationInfoRepository.CreateVerificationInfo(verificationInfo);

                // Send mail
                var result = _mailSender.SendVerifyEmail(
                    userData.Data.Email,
                    string.Empty,
                    token,
                    "[ArtJourney] – Email verification"
                );

                if (result == false)
                {
                    ApiResponse<string> emailSendingFailResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 500,
                        Errors =
                            [
                                new ApiError{Code = 1014}
                            ]
                    };
                }

                ApiResponse<string> successResponse = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = "Success send email"
                };

                return successResponse;
            }
            catch (UnauthorizedAccessException ex) when (ex.Message == "User Id not found in token")
            {
                
                Console.WriteLine("Error when get user_id by token: " + ex.Message);
                ApiResponse<string> errorUserIdResponse = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 401
                };
                return errorUserIdResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ApiResponse<string> response = new()
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Errors =
                        [
                            new ApiError {Code = 500}
                        ]
                };
                return response;
            }
            
        }

        public async Task<ApiResponse<string>> VerifyEmail(string token)
        {
            try
            {
                // Find in the db verification_info a record where the value of the token matches the value of v
                VerificationInfo? verificationInfo = await _verificationInfoRepository.GetVerificationInfoByEmail(token);
                if (verificationInfo == null)
                {
                    ApiResponse<string> viNotFoundErrorResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors =
                            [
                                new ApiError{Code = 1014}
                            ]
                    };
                    return viNotFoundErrorResponse;
                }

                // Compare ExpiresAt with UTC.Now
                DateTime utcTimeNow = DateTime.UtcNow;
                DateTime expiresTime = (DateTime)verificationInfo.ExpiresAt;
                int diffTime = DateTime.Compare(utcTimeNow, expiresTime);
                if (diffTime > 0)
                {
                    ApiResponse<string> viNotFoundErrorResponse = new()
                    {
                        Status = ResponseStatus.Error,
                        Code = 401,
                        Errors =
                            [
                                new ApiError{Code = 1014}
                            ]
                    };
                    await _verificationInfoRepository.DeleteVerificationInfosByEmail(verificationInfo.Email);
                    return viNotFoundErrorResponse;
                }

                // UPdate user status from "Pending" to "Active"
                NewUpdateUserDTO newUpdateUserDTO = new()
                {
                    Email = verificationInfo.Email,
                    Status = AccountStatus.Active
                };

                await _userService.UpdateUserAsync(newUpdateUserDTO);
                await _verificationInfoRepository.DeleteVerificationInfosByEmail(verificationInfo.Email);

                return new ApiResponse<string> 
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = null,
                    Message = "2004"
                };
            } catch (Exception ex) 
            { 
                Console.WriteLine(ex);
                ApiResponse<string> response = new()
                {
                    Code = 500,
                    Status = ResponseStatus.Error,
                    Errors =
                        [
                            new ApiError {Code = 1015}
                        ]
                };
                return response;
            }
            

        }

        //test
        public async Task<ApiResponse<int>> TestDeleteVerificationInfosByEmail(string email)
        {
            try
            {
                ApiResponse<int> response = new();
                if (email == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Code = 404;
                    response.Errors =
                        [
                            new ApiError{Code = 1111 }
                        ];
                    return response;
                }
                var deletedRecord = await _verificationInfoRepository.DeleteVerificationInfosByEmail(email);
                response.Status = ResponseStatus.Success;
                response.Code = 200;
                response.Data = deletedRecord;
                return response;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ApiResponse<int> response = new()
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors =
                        [
                            new ApiError{Code = 2222}
                        ]
                };
                return response;
            }
        }
    }
}
