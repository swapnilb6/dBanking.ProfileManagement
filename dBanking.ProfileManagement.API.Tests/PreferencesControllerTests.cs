
// dBanking.ProfileManagement.API.Tests/PreferencesControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using dBanking.ProfileManagement.API.Tests.Infrastructure;
using dBanking.ProfileManagement.Core.DTOs;
using FluentAssertions;
using Moq;
using Xunit;

namespace dBanking.ProfileManagement.API.Tests
{
    public class PreferencesControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public PreferencesControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPreferences_ShouldReturn200()
        {
            var customerId = Guid.NewGuid();
            var dto = new PreferencesDto
            {
                SmsEnabled = true,
                EmailEnabled = true,
                PushEnabled = false,
                RegulatoryConsentGiven = true,
                Language = "en-IN",
                TimeZone = "Asia/Kolkata",
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _factory.PreferenceServiceMock
                .Setup(s => s.GetAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var req = new HttpRequestMessage(HttpMethod.Get, $"/profiles/{customerId}/preferences");
            req.Headers.Add("X-Test-Scopes", "profile.read");

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadFromJsonAsync<PreferencesDto>();
            body!.Language.Should().Be("en-IN");
        }

        [Fact]
        public async Task UpdatePreferences_ShouldReturn200()
        {
            var customerId = Guid.NewGuid();
            var input = new UpdatePreferencesRequestDto
            {
                CustomerId = customerId,
                EmailEnabled = false,
                SourceChannel = "Web",
                CorrelationId = "cid-2"
            };

            var ret = new PreferencesDto { SmsEnabled = true, EmailEnabled = false, PushEnabled = false, RegulatoryConsentGiven = false, UpdatedAt = DateTimeOffset.UtcNow };

            _factory.PreferenceServiceMock
                .Setup(s => s.UpdateAsync(input, It.IsAny<Core.Entities.ActorContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ret);

            var req = new HttpRequestMessage(HttpMethod.Put, $"/profiles/{customerId}/preferences");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadFromJsonAsync<PreferencesDto>();
            body!.EmailEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task UpdatePreferences_ShouldReturn403_WhenMissingWriteScope()
        {
            var customerId = Guid.NewGuid();
            var input = new UpdatePreferencesRequestDto { CustomerId = customerId, EmailEnabled = false };

            var req = new HttpRequestMessage(HttpMethod.Put, $"/profiles/{customerId}/preferences");
            req.Headers.Add("X-Test-Scopes", "profile.read"); // no write scope
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);
            res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
