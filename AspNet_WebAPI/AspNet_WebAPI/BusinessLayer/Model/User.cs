using System.ComponentModel.DataAnnotations;

namespace AspNet_WebAPI.BusinessLayer.Model
{    
    public class DataStorage
    {
        public List<UserStorage> User { get; set; }
        public List<UserTokenInfo> UserTokenInfo { get; set; }
    }

    public class UserStorage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Policy { get; set; }
    }
}
