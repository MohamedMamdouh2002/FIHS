﻿using AutoMapper;
using FIHS.Dtos.UserDtos;
using FIHS.Interfaces;
using FIHS.Interfaces.IUser;
using FIHS.Models.AuthModels;
using Microsoft.AspNetCore.Identity;

namespace FIHS.Services.UserServices
{
    public class UserService : BaseService, IUserService
    {
        private readonly string _baseUrl;
        public UserService(UserManager<ApplicationUser> userManager,
            IImageService imageService, IMapper mapper, IConfiguration configuration) : base(context: null, userManager, mapper, imageService)
        {
            _baseUrl = configuration["BaseUrl"];
        }

        private UserDto MapUserToDto(ApplicationUser user)
        {
            var userDto = _mapper.Map<UserDto>(user);
            userDto.ProfilePicture = _baseUrl + user.ImgUrl;
            userDto.Succeeded = true;
            return userDto;
        }

        public async Task<UserDto> GetProfileAsync(string refreshToken)
        {
            var user = await GetUserByRefreshToken(refreshToken);

            if (user == null)
                return new UserDto { Succeeded = false, Message = "Invalid token." };

            return MapUserToDto(user);
        }

        public async Task<UserDto> UpdateProfileAsync(string refreshToken, UpdateProfileModel model)
        {
            var user = await GetUserByRefreshToken(refreshToken);

            if (user == null)
                return new UserDto { Succeeded = false, Message = "Invalid token." };

            user.UserName = model.Username ?? user.UserName;
            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(r => r.Description).ToList();
                string errorMessage = string.Join(", ", errors);
                return new UserDto { Succeeded = false, Message = errorMessage };
            }

            return MapUserToDto(user);
        }

        public async Task<UserDto> DeleteAccountAsync(string refreshToken)
        {
            var user = await GetUserByRefreshToken(refreshToken);

            if (user == null)
                return new UserDto { Succeeded = false, Message = "المستخدم غير موجود" };

            _imageService.DeleteImage(user.ImgUrl);
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return new UserDto { Succeeded = false, Message = "حدث خطأ ما" };

            return new UserDto { Succeeded = true };
        }

        public async Task<UserDto> SetImageAsync(string refreshToken, IFormFile imgFile)
        {
            var user = await GetUserByRefreshToken(refreshToken);

            if (user == null)
                return new UserDto { Succeeded = false, Message = "المستخدم غير موجود" };

            user.ImgUrl = _imageService.SetImage(imgFile, user.ImgUrl);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return new UserDto { Succeeded = false, Message = "حدث خطأ ما" };

            return new UserDto { Succeeded = true };
        }

        public async Task<UserDto> DeleteImageAsync(string refreshToken)
        {
            var user = await GetUserByRefreshToken(refreshToken);

            if (user == null)
                return new UserDto { Succeeded = false, Message = "المستخدم غير موجود" };
            
            _imageService.DeleteImage(user.ImgUrl);

            user.ImgUrl = "\\images\\Default_User_Image.png";
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return new UserDto { Succeeded = false, Message = "حدث خطأ ما" };

            return new UserDto { Succeeded = true };
        }
    }
}
