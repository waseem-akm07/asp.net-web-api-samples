using AspNet_WebAPI.BusinessLayer.Interfaces;
using AspNet_WebAPI.BusinessLayer.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        private static readonly object _fileLock = new object();

        public async Task<LoginUserDeatils> Login(string email, string password)
        {
            LoginUserDeatils result = new LoginUserDeatils();
            DataStorage data;
            UserStorage user;
            var path = Path.Combine(Environment.CurrentDirectory, "Storage/Storage.json");

            // Synchronize access to the file
            lock (_fileLock)
            {
                // Read the file
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    data = JsonSerializer.Deserialize<DataStorage>(fileStream);
                    user = data.User.FirstOrDefault(x => x.UserName == email && x.Password == password);

                    if (user == null)
                    {
                        return null;
                    }
                }

                // Generate the token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.GivenName, user.UserName),
                        new Claim(ClaimTypes.Email, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("Policy", user.Policy)
                    }),
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _configuration["JWT:Issuer"],
                    Audience = _configuration["JWT:Audience"],
                    Expires = DateTime.UtcNow.AddMinutes(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var refreshToken = GenerateRefreshToken().Result;

                // Add the refresh token to the data
                UserTokenInfo userTokenInfo = new UserTokenInfo
                {
                    Id = data.UserTokenInfo.Count() + 1,
                    EmailAdress = email,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = DateTime.UtcNow.AddHours(1)
                };

                data.UserTokenInfo.Add(userTokenInfo);

                // Write the updated data back to the file
                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(path, json);

                // Set the result
                result.Token = tokenHandler.WriteToken(token);
                result.IsActive = true;
                result.Name = user.Name;
                result.UserName = user.UserName;
                result.Role = user.Role;
                result.RefreshToken = refreshToken;
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
                Policy = user.Policy
            };

            data.User.Add(userStorage);

            // Write to the file after closing the read stream
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(path, json);

            return true;
        }

        public async Task<LoginUserDeatils> GenerateRefreshToken(string email, string refreshToken)
        {
            UserStorage user = new UserStorage();
            UserTokenInfo userTokenInfo = new UserTokenInfo();
            DataStorage dataStorage = new DataStorage();
            LoginUserDeatils result = new LoginUserDeatils();

            var path = Path.Combine(Environment.CurrentDirectory, "Storage/Storage.json");
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                dataStorage = JsonSerializer.Deserialize<DataStorage>(fileStream);
                userTokenInfo = dataStorage.UserTokenInfo.Where(x => x.EmailAdress == email).FirstOrDefault();

                if (userTokenInfo == null || userTokenInfo.RefreshToken != refreshToken || userTokenInfo.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    throw new Exception("Invalid access token or refresh token");
                }
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]);
            var tokenDiscriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.GivenName, user.UserName),
                    new Claim(ClaimTypes.Email, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Policy", user.Policy)
                }),
                IssuedAt = DateTime.UtcNow,
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var newToken = tokenHandler.CreateToken(tokenDiscriptor);
            var newRefreshToken = await GenerateRefreshToken();

            UserTokenInfo userToken = new UserTokenInfo()
            {
                Id = dataStorage.UserTokenInfo.Count() + 1,
                EmailAdress = email,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddHours(1)
            };

            using (FileStream fileStream2 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var data = JsonSerializer.Deserialize<DataStorage>(fileStream2);
                data.UserTokenInfo.Add(userTokenInfo);
                var json = JsonSerializer.Serialize(data);
                await File.WriteAllTextAsync(path, json);
            }

            result.Token = tokenHandler.WriteToken(newToken);
            result.IsActive = true;
            result.Name = user.Name;
            result.UserName = user.UserName;
            result.Role = user.Role;
            result.RefreshToken = refreshToken;
            return result;
        }

        public async Task<ClaimsPrincipal> GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]);

            var validationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principle = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);

            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
            if(jwtSecurityToken == null || jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }
            return principle;
        }

        private async Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            var refreshToken = Convert.ToBase64String(randomNumber);
            return refreshToken;
        }
    }
}
