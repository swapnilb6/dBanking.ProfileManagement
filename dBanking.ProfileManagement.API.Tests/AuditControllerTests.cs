using System.Net;
using System.Net.Http.Json;
using dBanking.ProfileManagement.API.Tests.Infrastructure;
using dBanking.ProfileManagement.Core.DTOs;
using FluentAssertions;
using Moq;
using Xunit;

namespace dBanking.ProfileManagement.API.Tests
{
    public class AuditControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuditControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAudit_ShouldReturn200_WithEntries()
        {
            var customerId = Guid.NewGuid();

            _factory.AuditServiceMock
                .Setup(s => s.GetAsync(customerId, "Contacts", 0, 50, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditEntryDto>
                {
                    new AuditEntryDto
                    {
                        AuditId = Guid.NewGuid(),
                        CustomerId = customerId,
                        Entity = "Contacts",
                        EntityId = "Email",
                        Operation = "Verified",
                        OldValueJson = "{}",
                        NewValueJson = "{}",
                        ChangedFieldsCsv = "Email",
                        ActorId = "test-user",
                        ActorRole = "Customer",
                        SourceChannel = "Web",
                        Timestamp = DateTimeOffset.UtcNow
                    }
                });

            var req = new HttpRequestMessage(HttpMethod.Get, $"/profiles/{customerId}/audit?entity=Contacts");
            req.Headers.Add("X-Test-Scopes", "profile.read");

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await res.Content.ReadFromJsonAsync<List<AuditEntryDto>>();
            list.Should().HaveCount(1);
            list![0].Entity.Should().Be("Contacts");
        }
    }
}