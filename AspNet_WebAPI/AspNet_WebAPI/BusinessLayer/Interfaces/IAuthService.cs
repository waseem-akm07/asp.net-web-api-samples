using AspNet_WebAPI.BusinessLayer.Model;

namespace AspNet_WebAPI.BusinessLayer.Interfaces
{
    public interface IAuthService
    {
        Task<User> Login(string email, string password);
        Task<bool> Register(RegisterUser user);
    }
}
