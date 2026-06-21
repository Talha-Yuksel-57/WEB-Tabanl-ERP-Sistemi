using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok(new
            {
                User = User.Identity?.Name
            });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("admin")]
        public IActionResult Admin()
        {
            return Ok("Admin erişimi başarılı");
        }
    }
}