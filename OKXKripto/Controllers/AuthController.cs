using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using OKXKripto.Services;

namespace OKXKripto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _userService.Authenticate(request.Username, request.Password);
            return Ok(new { Token = token });
        }
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }


    }
}
