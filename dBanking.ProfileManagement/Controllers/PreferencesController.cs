
// Controllers/PreferencesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.API.Extensions;

namespace dBanking.ProfileManagement.API.Controllers
{
    [ApiController]
    [Route("profiles/{customerId:guid}/preferences")]
    public sealed class PreferencesController : ControllerBase
    {
        private readonly IPreferenceService _prefs;

        public PreferencesController(IPreferenceService prefs)
        {
            _prefs = prefs;
        }

        [HttpGet]
        [Authorize(Policy = "profile.read")]
        [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid customerId, CancellationToken ct)
        {
            var dto = await _prefs.GetAsync(customerId, ct);
            return Ok(dto);
        }

        [HttpPut]
        [Authorize(Policy = "profile.write")]
        [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(Guid customerId, [FromBody] UpdatePreferencesRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId) return BadRequest(new { message = "customerId mismatch." });

            var actor = ActorContextFactory.From(HttpContext, request.Reason);
            var dto = await _prefs.UpdateAsync(request, actor, ct);
            return Ok(dto);
        }
    }
}
