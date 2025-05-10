using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.Users;
using Helpers.HelperClasses;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class AuthenService : IAuthenService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        public AuthenService(IUserService userService, ITokenService tokenService) 
        {
            _userService = userService;
            _tokenService = tokenService;
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

        public async Task<ApiResponse<AuthenticationResponse>> Login(LoginDTO loginDto)
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
                ApiResponse<User> response = new()
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                };
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        Fullname = name ?? "",
                        AvatarUrl = avatar ?? ""
                    };
                    response.Data = await _userService.CreateCompleteAccount(user); // B13: Tạo mới người dùng
                }
                else
                {
                    // Check if this email has already registered by another user
                    if (!string.IsNullOrWhiteSpace(user.Password))
                    {
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
                    ApiResponse<User> updatedUserResponse = await _userService.UpdateUserAsync(newUpdateUserDTO);
                    if (updatedUserResponse.Status.Equals(ResponseStatus.Error))
                    {
                        return updatedUserResponse;
                    }
                    response.Data = updatedUserResponse.Data;
                }

                if (response.Data == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Code = 500;
                    response.Errors =
                        [
                            new ApiError {Code = 1005}
                        ];

                    return response;
                }

                response.Message = _tokenService.CreateToken(response.Data);
                return response;
            }
            catch (Exception ex)
            {
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

    }
}
