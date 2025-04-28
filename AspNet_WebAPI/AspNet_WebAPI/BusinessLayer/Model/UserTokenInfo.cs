namespace AspNet_WebAPI.BusinessLayer.Model
{
    public class LoginUserDeatils
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class LoginUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RegisterUser
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Policy { get; set; }
    }

    public class UserTokenInfo
    {
        public int Id { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public string EmailAdress { get; set; }
    }

    public class TokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
