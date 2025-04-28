using AspNet_WebAPI.BusinessLayer.Interfaces;
using AspNet_WebAPI.BusinessLayer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNet_WebAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        [MapToApiVersion("3.0")]
        public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
        {
            var user = await _authService.Login(loginUser.UserName, loginUser.Password);
            if(user != null)
            {
                return Ok(user);
            }

            return BadRequest(new { message = "User login unsuccessful" });
        }

        [HttpPost]
        [Route("register")]
        [Authorize(Policy = "Write")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Register([FromBody] RegisterUser user)
        {
            var result = await _authService.Register(user);
            if (result == true)
            {
                return Ok("Registered successfully");
            }
            else
            {
                return BadRequest("Registration Failed");
            }
        }

        [HttpPost]
        [Route("refreshtoken")]
        [Authorize("ContainRole")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        [MapToApiVersion("3.0")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if(tokenModel != null)
            {
                return BadRequest("Invalid Details");
            }

            var principle =await _authService.GetPrincipleFromExpiredToken(tokenModel.Token);
            if(principle == null)
            {
                return BadRequest("Invalid Access token or Refresh token");
            }

            var tokenDetails = await _authService.GenerateRefreshToken(principle.Identity.Name, tokenModel.RefreshToken);
            return Ok(tokenDetails);
        }
    }
}
