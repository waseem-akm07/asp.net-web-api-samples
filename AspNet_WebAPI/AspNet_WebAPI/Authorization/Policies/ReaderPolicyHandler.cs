using AspNet_WebAPI.BusinessLayer.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace AspNet_WebAPI.Authorization.Policies
{
    public class ReaderPolicyHandler : AuthorizationHandler<ReaderRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ReaderRequirement requirement)
        {
            var data = new DataStorage();
            if (!context.User.HasClaim(x => x.Type == ClaimTypes.Email))
                return Task.CompletedTask;

            var policyClaim = context.User.Claims.FirstOrDefault(x => x.Type == "Policies");
            var emailClaim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

            var path = Path.Combine(Environment.CurrentDirectory, "Storage/Storage.json");
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                data = JsonSerializer.Deserialize<DataStorage>(fileStream);
            }

            var userInfo = data.User.Where(x => x.UserName == emailClaim.Value).FirstOrDefault();
            if (userInfo != null)
            {
                foreach (var policy in userInfo?.Policies ?? new List<string>())
                {
                    if (!string.IsNullOrEmpty(policy) && policyClaim != null && !string.IsNullOrEmpty(policyClaim.Value) && policyClaim.Value.Contains(policy))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
