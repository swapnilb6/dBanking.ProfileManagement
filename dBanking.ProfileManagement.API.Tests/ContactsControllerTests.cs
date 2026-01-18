
// dBanking.ProfileManagement.API.Tests/ContactsControllerTests.cs
using dBanking.ProfileManagement.API.Tests.Infrastructure;
using dBanking.ProfileManagement.Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace dBanking.ProfileManagement.API.Tests
{
    public class ContactsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ContactsControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetContacts_ShouldReturn200_WithDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var dto = new ContactViewDto
            {
                Email = "user@example.com",
                EmailStatus = "Verified",
                PhoneE164 = "+911234567890",
                PhoneStatus = "Verified",
                PendingEmail = null,
                PendingPhoneE164 = null
            };

            _factory.ContactServiceMock
                .Setup(s => s.GetAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, $"/profiles/{customerId}/contacts");
            request.Headers.Add("X-Test-Scopes", "profile.read");
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ContactViewDto>();
            body.Should().NotBeNull();
            body!.Email.Should().Be("user@example.com");
        }

        [Fact]
        public async Task RequestEmailChange_ShouldReturn202_WhenValid_AndIdempotencyProvided()
        {
            var customerId = Guid.NewGuid();
            var input = new ChangeEmailRequestDto
            {
                CustomerId = customerId,
                NewEmail = "new@example.com",
                SourceChannel = "Web",
                CorrelationId = "cid-1",
                Reason = "test"
            };
            var result = new ContactChangeResultDto
            {
                Success = true,
                Status = "PendingVerification",
                Message = "Verification link sent"
            };

            _factory.ContactServiceMock
                .Setup(s => s.RequestEmailChangeAsync(input, It.IsAny<Core.Entities.ActorContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{customerId}/contacts/email/change-request");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Headers.Add("Idempotency-Key", "key-123");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.Accepted);
            var body = await res.Content.ReadFromJsonAsync<ContactChangeResultDto>();
            body!.Status.Should().Be("PendingVerification");

            _factory.ContactServiceMock.Verify(s =>
                s.RequestEmailChangeAsync(It.Is<ChangeEmailRequestDto>(d => d.NewEmail == "new@example.com"),
                    It.IsAny<Core.Entities.ActorContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RequestEmailChange_ShouldReturn400_WhenIdempotencyMissing()
        {
            var customerId = Guid.NewGuid();
            var input = new ChangeEmailRequestDto
            {
                CustomerId = customerId,
                NewEmail = "new@example.com",
                SourceChannel = "Web"
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{customerId}/contacts/email/change-request");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RequestEmailChange_ShouldReturn403_WhenMissingWriteScope()
        {
            var customerId = Guid.NewGuid();
            var input = new ChangeEmailRequestDto
            {
                CustomerId = customerId,
                NewEmail = "new@example.com",
                SourceChannel = "Web"
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{customerId}/contacts/email/change-request");
            req.Headers.Add("X-Test-Scopes", "profile.read"); // no write scope
            req.Headers.Add("Idempotency-Key", "k1");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturn200_WhenSuccess()
        {
            var customerId = Guid.NewGuid();
            var input = new VerifyEmailRequestDto
            {
                CustomerId = customerId,
                VerificationToken = "token"
            };
            var ok = new OperationResultDto { Success = true, Code = "OK", Message = "done" };

            _factory.ContactServiceMock
                .Setup(s => s.VerifyEmailAsync(input, It.IsAny<Core.Entities.ActorContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ok);

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{customerId}/contacts/email/verify");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await res.Content.ReadFromJsonAsync<OperationResultDto>();
            body!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturn400_OnCustomerIdMismatch()
        {
            var routeCustomer = Guid.NewGuid();
            var bodyCustomer = Guid.NewGuid();
            var input = new VerifyEmailRequestDto
            {
                CustomerId = bodyCustomer,
                VerificationToken = "token"
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"/profiles/{routeCustomer}/contacts/email/verify");
            req.Headers.Add("X-Test-Scopes", "profile.write");
            req.Content = JsonContent.Create(input);

            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}