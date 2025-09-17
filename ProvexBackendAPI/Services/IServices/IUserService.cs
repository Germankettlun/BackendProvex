using ProvexBackendAPI.Data.Models.Users;
using System;
using System.Collections.Generic;
using static ProvexBackendAPI.Dto.Users.UsersDto;


namespace ProvexBackendAPI.Services.IServices
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsers();
        Task<UserDto?> GetUser(Guid id);
        Task<bool> IsUniqueUser(string username);
    }
}