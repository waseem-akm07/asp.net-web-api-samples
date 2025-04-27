using System.ComponentModel.DataAnnotations;

namespace AspNet_WebAPI.BusinessLayer.Model
{
    public class User
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
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
        public List<string> Policies { get; set; }
    }

    public class DataStorage
    {
        public List<UserStorage> User { get; set; }
    }

    public class UserStorage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<string> Policies { get; set; }
    }
}
