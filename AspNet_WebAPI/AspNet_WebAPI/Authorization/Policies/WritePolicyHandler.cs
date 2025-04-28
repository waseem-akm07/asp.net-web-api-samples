using AspNet_WebAPI.BusinessLayer.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace AspNet_WebAPI.Authorization.Policies
{
    public class WritePolicyHandler : AuthorizationHandler<WriterRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WriterRequirement requirement)
        {
            var data = new DataStorage();
            if (!context.User.HasClaim(x => x.Type == ClaimTypes.Email))
                return Task.CompletedTask;

            var policyClaim = context.User.Claims.FirstOrDefault(x => x.Type == "Policy");
            var emailClaim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

            var path = Path.Combine(Environment.CurrentDirectory, "Storage/Storage.json");
            using(FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                data = JsonSerializer.Deserialize<DataStorage>(fileStream);
            }

            var userInfo = data.User.Where(x => x.UserName == emailClaim.Value).FirstOrDefault();
            if(userInfo != null)
            {
                if (!string.IsNullOrEmpty(userInfo.Policy) && policyClaim != null && !string.IsNullOrEmpty(policyClaim.Value) && policyClaim.Value.Contains(userInfo.Policy))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
