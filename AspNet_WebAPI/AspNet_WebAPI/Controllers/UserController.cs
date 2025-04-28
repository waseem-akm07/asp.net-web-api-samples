using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNet_WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "Read")]
        [MapToApiVersion("2.0")]
        public IEnumerable<string> Get()
        {
            return new string[] { "User 1", "User 2" };
        }

        [HttpGet("{id}")]
        [Authorize("ContainRole")]
        [MapToApiVersion("3.0")]
        public string Get(int id)
        {
            return "User " + id;
        }
    }
}
