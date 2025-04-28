using AspNet_WebAPI.BusinessLayer.Model;
using System.Security.Claims;

namespace AspNet_WebAPI.BusinessLayer.Interfaces
{
    public interface IAuthService
    {
        Task<LoginUserDeatils> Login(string email, string password);
        Task<bool> Register(RegisterUser user);
        Task<LoginUserDeatils> GenerateRefreshToken(string email, string refreshToken);
        Task<ClaimsPrincipal> GetPrincipleFromExpiredToken(string token);
    }
}
