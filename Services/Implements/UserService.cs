using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.User;
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
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(
            IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        public async Task<User> CreateAccount(RegisterDTO registerDTO)
        {
            var user = new User() 
            { 
                Email = registerDTO.Email,
                Password = PasswordHasher.HashPassword(registerDTO.Password),
                Role = registerDTO.Role,
            };
            var createdUser = await _userRepository.CreateUserAsync(user);
            return createdUser;
        }

        public async Task<User> CreateCompleteAccount(User newUser)
        {
            var createdUser = await _userRepository.CreateUserAsync(newUser);
            return createdUser;
        }



        public Task<User?> GetUserByEmailAsync(string email)
        {
            var foundUser = _userRepository.GetUserByEmailAsync(email);
            return foundUser;
        }

        public async Task<ApiResponse<User>> UpdateUserAsync(NewUpdateUserDTO newUpdateUser)
        {
            try 
            {
                if (newUpdateUser.Email == null)
                {
                    return new ApiResponse<User>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors =
                        [
                            new ApiError { Code = 1002, Message = "Email not found" }
                        ]
                    };
                }

                var updatedUser = await _userRepository.GetUserByEmailAsync(newUpdateUser.Email);
                if (updatedUser == null)
                {
                    return new ApiResponse<User>
                    {
                        Status = ResponseStatus.Error,
                        Code = 404,
                        Errors =
                        [
                            new ApiError { Code = 1001, Message = "No user found with this email" }
                        ]
                    };
                }
                bool isUpdate = false;
                // check email
                if (newUpdateUser.Email != null)
                {
                    if (!newUpdateUser.Email.Equals(updatedUser.Email))
                    {
                        updatedUser.Email = newUpdateUser.Email;
                        isUpdate = true;
                    }
                }
                //check fullname
                if (newUpdateUser.FullName != null)
                {
                    if (!newUpdateUser.FullName.Equals(updatedUser.Fullname))
                    {
                        updatedUser.Fullname = newUpdateUser.FullName;
                        isUpdate = true;
                    }
                }
                //check Gender
                if (newUpdateUser.Gender != null)
                {
                    if (!newUpdateUser.Gender.Equals(updatedUser.Gender))
                    {
                        Gender newUpdatedUserGender = (Gender)newUpdateUser.Gender;
                        updatedUser.Gender = newUpdatedUserGender;
                        isUpdate = true;
                    }
                }
                //PhoneNumber
                if (newUpdateUser.PhoneNumber != null)
                {
                    if (!newUpdateUser.PhoneNumber.Equals(updatedUser.PhoneNumber))
                    {
                        updatedUser.PhoneNumber = newUpdateUser.PhoneNumber;
                        isUpdate = true;
                    }
                }
                //Password
                if (newUpdateUser.Password != null)
                {
                    if (!newUpdateUser.Password.Equals(updatedUser.Password))
                    {
                        updatedUser.Password = newUpdateUser.Password;
                        isUpdate = true;
                    }
                }
                //Birthday
                if (newUpdateUser.Birthday != null)
                {
                    if (!newUpdateUser.Birthday.Equals(updatedUser.Birthday))
                    {
                        DateTime newUpdateUserBirthday = (DateTime)newUpdateUser.Birthday;
                        updatedUser.Birthday = newUpdateUserBirthday;
                        isUpdate = true;
                    }
                }
                // check status
                if (newUpdateUser.Status != null)
                {
                    if (!newUpdateUser.Status.Equals(updatedUser.Status))
                    {
                        AccountStatus newUpdateUserStatus = (AccountStatus)newUpdateUser.Status;
                        updatedUser.Status = newUpdateUserStatus;
                        isUpdate = true;
                    }
                }
                //check avatarUrl
                if (newUpdateUser.AvatarUrl != null)
                {
                    if (!newUpdateUser.AvatarUrl.Equals(updatedUser.AvatarUrl))
                    {
                        updatedUser.AvatarUrl = newUpdateUser.AvatarUrl;
                        isUpdate = true;
                    }
                }

                if (isUpdate)
                {
                    updatedUser.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(updatedUser);
                    
                }
                return new ApiResponse<User> 
                {
                    Status = ResponseStatus.Success,
                    Code = 200,
                    Data = updatedUser
                };
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ApiResponse<User>
                {
                    Status = ResponseStatus.Error,
                    Code = 500,
                    Errors =
                    [
                        new ApiError {Code = 1003}
                    ]
                };
            }
        }
    }
}
