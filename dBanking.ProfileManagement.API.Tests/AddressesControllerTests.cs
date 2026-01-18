using System.Net;
using System.Net.Http.Json;
using dBanking.ProfileManagement.API.Tests.Infrastructure;
using dBanking.ProfileManagement.Core.DTOs;
using FluentAssertions;
using Moq;
using Xunit;

namespace dBanking.ProfileManagement.API.Tests
{
    public class AddressesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AddressesControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAddresses_ShouldReturn200_WithList()
        {
            var customerId = Guid.NewGuid();

            _factory.AddressServiceMock
                .Setup(s => s.GetByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AddressDto>
                {
                    new AddressDto { AddressId = Guid.NewGuid(), AddressType = "Residential", Line1 = "L1", City="Pune", StateProvince="MH", PostalCode="411001", CountryCode="IN" }
                });

            var req = new HttpRequestMessage(HttpMethod.Get, $"/profiles/{customerId}/addresses");
            req.Headers.Add("X-Test-Scopes", "profile.read");

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await res.Content.ReadFromJsonAsync<List<AddressDto>>();
            list.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpsertAddress_ShouldReturn200_AndInvokeService()
        {
            var customerId = Guid.NewGuid();
            var input = new UpsertAddressRequestDto
            {
                CustomerId = customerId,
                AddressType = "Residential",
                Line1 = "Flat 1",
                City = "Pune",
                StateProvince = "MH",
                PostalCode = "411001",
                CountryCode = "IN",
                IsPrimary = true
            };
            var ret = new AddressDto
            {
                AddressId = Guid.NewGuid(),
                AddressType = "Residential",
                Line1 = "Flat 1",
                City = "Pune",
                StateProvince = "MH",
                PostalCode = "411001",
                CountryCode = "IN",
                IsPrimary = true
            };

            _factory.AddressServiceMock
                .Setup(s => s.UpsertAsync(input, It.IsAny<Core.Entities.ActorContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ret);

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{customerId}/addresses");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Headers.Add("Idempotency-Key", "addr-upsert-1");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await res.Content.ReadFromJsonAsync<AddressDto>();
            dto!.AddressType.Should().Be("Residential");

            _factory.AddressServiceMock.Verify(s => s.UpsertAsync(
                It.Is<UpsertAddressRequestDto>(d => d.Line1 == "Flat 1"),
                It.IsAny<Core.Entities.ActorContext>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAddress_ShouldReturn400_OnRouteBodyMismatch()
        {
            var customerId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            var input = new UpdateAddressRequestDto
            {
                CustomerId = Guid.NewGuid(), // mismatch
                AddressId = addressId,
                AddressType = "Residential",
                Line1 = "New",
                City = "Pune",
                StateProvince = "MH",
                PostalCode = "411001",
                CountryCode = "IN",
                IsPrimary = true,
                EffectiveFrom = DateTimeOffset.UtcNow
            };

            var req = new HttpRequestMessage(HttpMethod.Put, $"/profiles/{customerId}/addresses/{addressId}");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SetPrimary_ShouldReturn200()
        {
            var customerId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            _factory.AddressServiceMock
                .Setup(s => s.SetPrimaryAsync(customerId, addressId, It.IsAny<Core.Entities.ActorContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OperationResultDto { Success = true, Code = "OK", Message = "Primary address updated." });

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{customerId}/addresses/{addressId}/set-primary");
            req.Headers.Add("X-Test-Scopes", "profile.write");

            var res = await _client.SendAsync(req);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}