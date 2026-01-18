using Microsoft.AspNetCore.Mvc;

namespace dBanking.ProfileManagement.API.Controllers
{
    [ApiController]
    public sealed class HealthController : ControllerBase
    {
        [HttpGet("/health")]
        public IActionResult Health() => Ok(new { status = "Healthy" });
    }
}
