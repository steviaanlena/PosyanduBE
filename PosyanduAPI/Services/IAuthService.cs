using PosyanduAPI.DTOs;
using PosyanduAPI.Models;

namespace PosyanduAPI.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<UserResponseDto>> Register(UserRegisterDto userRegisterDto);
        Task<ServiceResponse<UserResponseDto>> Login(UserLoginDto userLoginDto, string userType);
        bool IsUserExist(string nik);
    }

    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}