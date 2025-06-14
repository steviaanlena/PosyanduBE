using Microsoft.AspNetCore.Mvc;
using PosyanduAPI.DTOs;
using PosyanduAPI.Services;

namespace PosyanduAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse<UserResponseDto>>> Register(UserRegisterDto userRegisterDto)
        {
            var response = await _authService.Register(userRegisterDto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login/kader")]
        public async Task<ActionResult<ServiceResponse<UserResponseDto>>> LoginKader(UserLoginDto userLoginDto)
        {
            var response = await _authService.Login(userLoginDto, "Kader");
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login/ortu")]
        public async Task<ActionResult<ServiceResponse<UserResponseDto>>> LoginOrtu(UserLoginDto userLoginDto)
        {
            var response = await _authService.Login(userLoginDto, "Ortu");
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}