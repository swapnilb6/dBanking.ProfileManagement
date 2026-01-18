using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.API.Extensions;
using dBanking.ProfileManagement.API.Filters;
using Microsoft.AspNetCore.RateLimiting;

namespace dBanking.ProfileManagement.API.Controllers
{
    [ApiController]
    [Route("profiles/{customerId:guid}/contacts")]
    public sealed class ContactsController : ControllerBase
    {
        private readonly IContactService _contacts;

        public ContactsController(IContactService contacts)
        {
            _contacts = contacts;
        }

        [HttpGet]
        [Authorize(Policy = "profile.read")]
        [ProducesResponseType(typeof(ContactViewDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid customerId, CancellationToken ct)
        {
            var dto = await _contacts.GetAsync(customerId, ct);
            return Ok(dto);
        }

        [HttpPost("email/change-request")]
        [Authorize(Policy = "profile.write")]
        [IdempotencyRequired]
        [EnableRateLimiting("tight")]
        [ProducesResponseType(typeof(ContactChangeResultDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestEmailChange(Guid customerId, [FromBody] ChangeEmailRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId) return BadRequest(new { message = "customerId mismatch." });

            var actor = ActorContextFactory.From(HttpContext, request.Reason);
            var result = await _contacts.RequestEmailChangeAsync(request, actor, ct);
            return Accepted(result);
        }

        [HttpPost("email/verify")]
        [Authorize(Policy = "profile.write")]
        [EnableRateLimiting("tight")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyEmail(Guid customerId, [FromBody] VerifyEmailRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId) return BadRequest(new { message = "customerId mismatch." });

            var actor = ActorContextFactory.From(HttpContext);
            var result = await _contacts.VerifyEmailAsync(request, actor, ct);
            return Ok(result);
        }

        [HttpPost("phone/change-request")]
        [Authorize(Policy = "profile.write")]
        [IdempotencyRequired]
        [EnableRateLimiting("tight")]
        [ProducesResponseType(typeof(ContactChangeResultDto), StatusCodes.Status202Accepted)]
        public async Task<IActionResult> RequestPhoneChange(Guid customerId, [FromBody] ChangePhoneRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId) return BadRequest(new { message = "customerId mismatch." });

            var actor = ActorContextFactory.From(HttpContext, request.Reason);
            var result = await _contacts.RequestPhoneChangeAsync(request, actor, ct);
            return Accepted(result);
        }

        [HttpPost("phone/verify")]
        [Authorize(Policy = "profile.write")]
        [EnableRateLimiting("tight")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyPhone(Guid customerId, [FromBody] VerifyPhoneRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId) return BadRequest(new { message = "customerId mismatch." });

            var actor = ActorContextFactory.From(HttpContext);
            var result = await _contacts.VerifyPhoneAsync(request, actor, ct);
            return Ok(result);
        }
    }
}
