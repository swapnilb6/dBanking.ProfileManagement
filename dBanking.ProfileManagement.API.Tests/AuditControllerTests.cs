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
            // Arrange
            var customerId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            _factory.AuditServiceMock
                .Setup(s => s.GetAsync(customerId, "Contacts", 0, 50, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditEntryDto>
                {
                    new AuditEntryDto
                    {
                        AuditId = 123L,                 // long (BIGSERIAL), not Guid
                        CustomerId = customerId,
                        EntityName = "Contacts",
                        EntityId = "Email",
                        Action = "Verified",
                        OldValueJson = "{}",
                        NewValueJson = "{}",
                        ChangedFieldsCsv = "Email",
                        Actor = "test-user",            // replaces ActorId
                        Channel = "API",                // replaces SourceChannel (canonical)
                        CorrelationId = Guid.NewGuid(),
                        ChangedAt = now                 // replaces Timestamp
                    }
                });

            var req = new HttpRequestMessage(HttpMethod.Get, $"/profiles/{customerId}/audit?entity=Contacts");
            // Test auth helper in your test host reads this header to attach scopes
            req.Headers.Add("X-Test-Scopes", "profile.read");

            // Act
            var res = await _client.SendAsync(req);

            // Assert
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await res.Content.ReadFromJsonAsync<List<AuditEntryDto>>();
            list.Should().NotBeNull();
            list!.Should().HaveCount(1);
            var item = list[0];

            item.AuditId.Should().Be(123L);
            item.CustomerId.Should().Be(customerId);
            item.EntityName.Should().Be("Contacts");
            item.EntityId.Should().Be("Email");
            item.Action.Should().Be("Verified");
            item.Actor.Should().Be("test-user");
            item.Channel.Should().Be("API");
            item.ChangedFieldsCsv.Should().Be("Email");
            item.ChangedAt.Should().BeCloseTo(now, precision: TimeSpan.FromSeconds(2));
        }
    }
}