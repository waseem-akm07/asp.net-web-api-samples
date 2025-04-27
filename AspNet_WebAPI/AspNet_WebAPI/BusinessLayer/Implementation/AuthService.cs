using AspNet_WebAPI.BusinessLayer.Interfaces;
using AspNet_WebAPI.BusinessLayer.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AspNet_WebAPI.BusinessLayer.Implementation
{
    //using BCrypt.Net
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<User> Login(string email, string password)
        {
            User result = new User();
            var path = Path.Combine(Environment.CurrentDirectory, "Storage/Storage.json");

            // Open the file with shared read access
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var data = JsonSerializer.Deserialize<DataStorage>(fileStream);
                var user = data.User.Where(x => x.UserName == email && x.Password == password).FirstOrDefault();

                if (user == null)
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"]);

                var tokenDiscriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.GivenName, user.UserName),
                        new Claim(ClaimTypes.Email, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("Policies", user?.Policies?.ToString() ?? "")
                    }),
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _configuration["JWT:Issuer"],
                    Audience = _configuration["JWT:Audience"],
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDiscriptor);
                result.Token = tokenHandler.WriteToken(token);
                result.IsActive = true;
                result.Name = user.Name;
                result.UserName = user.UserName;
                result.Role = user.Role;
            }

            return result;
        }

        public async Task<bool> Register(RegisterUser user)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Storage/Storage.json");

            // Open the file with shared read access
            DataStorage data;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                data = JsonSerializer.Deserialize<DataStorage>(fileStream);
            }

            UserStorage userStorage = new UserStorage
            {
                Name = user.Name,
                Id = data?.User?.Count() + 1 ?? 0,
                UserName = user.UserName,
                Password = user.Password,
                Role = user.Role,
                Policies = user.Policies
            };

            data.User.Add(userStorage);

            // Write to the file after closing the read stream
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(path, json);

            return true;
        }
    }
}
