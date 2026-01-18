
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.DTOs;

namespace dBanking.ProfileManagement.API.Controllers
{
    [ApiController]
    [Route("profiles/{customerId:guid}/audit")]
    public sealed class AuditController : ControllerBase
    {
        private readonly IAuditService _audit;

        public AuditController(IAuditService audit)
        {
            _audit = audit;
        }

        [HttpGet]
        [Authorize(Policy = "profile.read")]
        [ProducesResponseType(typeof(IEnumerable<AuditEntryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid customerId, [FromQuery] string? entity, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        {
            var items = await _audit.GetAsync(customerId, entity, skip, take, ct);
            return Ok(items);
        }
    }
}
