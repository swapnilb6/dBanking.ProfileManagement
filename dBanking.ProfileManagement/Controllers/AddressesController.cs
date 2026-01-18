using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.API.Extensions;
using dBanking.ProfileManagement.API.Filters;

namespace dBanking.ProfileManagement.API.Controllers
{
    [ApiController]
    [Route("profiles/{customerId:guid}/addresses")]
    public sealed class AddressesController : ControllerBase
    {
        private readonly IAddressService _addresses;

        public AddressesController(IAddressService addresses)
        {
            _addresses = addresses;
        }

        [HttpGet]
        [Authorize(Policy = "profile.read")]
        [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid customerId, CancellationToken ct)
        {
            var list = await _addresses.GetByCustomerAsync(customerId, ct);
            return Ok(list);
        }

        [HttpPost]
        [Authorize(Policy = "profile.write")]
        [IdempotencyRequired]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Upsert(Guid customerId, [FromBody] UpsertAddressRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId) return BadRequest(new { message = "customerId mismatch." });

            var actor = ActorContextFactory.From(HttpContext, request.Reason);
            var dto = await _addresses.UpsertAsync(request, actor, ct);
            return Ok(dto);
        }

        [HttpPut("{addressId:guid}")]
        [Authorize(Policy = "profile.write")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid customerId, Guid addressId, [FromBody] UpdateAddressRequestDto request, CancellationToken ct)
        {
            if (request.CustomerId != customerId || request.AddressId != addressId)
                return BadRequest(new { message = "customerId/addressId mismatch." });

            var actor = ActorContextFactory.From(HttpContext, request.Reason);
            var dto = await _addresses.UpdateAsync(request, actor, ct);
            return Ok(dto);
        }

        [HttpPost("{addressId:guid}/set-primary")]
        [Authorize(Policy = "profile.write")]
        [ProducesResponseType(typeof(OperationResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetPrimary(Guid customerId, Guid addressId, CancellationToken ct)
        {
            var actor = ActorContextFactory.From(HttpContext);
            var result = await _addresses.SetPrimaryAsync(customerId, addressId, actor, ct);
            return Ok(result);
        }
    }
}
