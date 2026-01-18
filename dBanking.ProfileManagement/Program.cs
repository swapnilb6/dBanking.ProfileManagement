using dBanking.ProfileManagement.API.Filters;
using dBanking.ProfileManagement.API.Middleware;
using dBanking.ProfileManagement.Core.Validators.Contacts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Logging (keep default or add Serilog/App Insights)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


builder.Services.AddControllers()
    .AddJsonOptions(o => { /* JSON opts if needed */ });

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Register validators from Core assembly
builder.Services.AddValidatorsFromAssemblyContaining<dBanking.ProfileManagement.Core.Validators.Contacts.ChangeEmailRequestValidator>();

// Optionally, standardize validation problem details
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Optionally format errors
        return new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
    };
});


// Authentication and Authorization

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.ValidAudiences = new[]
        {
            builder.Configuration["AzureAd:Audience"],
            builder.Configuration["AzureAd:ClientId"] // in case tokens use clientId as aud
        };
    },
    options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });

// Policies based on OAuth scopes (scp claim)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("profile.read", policy => policy.RequireScope("profile.read"));
    options.AddPolicy("profile.write", policy => policy.RequireScope("profile.write"));
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Swagger with OAuth2
// Swagger
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "dBanking Profile Management API", Version = "v1" });

    o.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    // IMPORTANT: Prefix with your Application ID URI
                    [$"{builder.Configuration["AzureAd:Audience"]}/profile.read"] = "Read profile",
                    [$"{builder.Configuration["AzureAd:Audience"]}/profile.write"] = "Write profile"
                }
            }
        }
    });

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" } }
        ] = new[] { $"{builder.Configuration["AzureAd:Audience"]}/profile.read",
                    $"{builder.Configuration["AzureAd:Audience"]}/profile.write" }
    });
});



builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(ChangeEmailRequestValidator).Assembly);
builder.Services.AddProblemDetails(); // .NET 8 built-in


// Idempotency store (in-memory for dev)
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();


// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("tight", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});


// API Versioning
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});



var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId("7eb2a262-7d80-4d2c-856b-efa4c44da4d4");
        c.OAuthUsePkce();
        c.OAuthScopes(
            $"{app.Configuration["AzureAd:Audience"]}/profile.read",
            $"{app.Configuration["AzureAd:Audience"]}/profile.write");
        c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "prompt", "select_account" } });
    });
}


app.Use(async (ctx, next) =>
{
    const string header = "X-Correlation-Id";
    if (!ctx.Request.Headers.TryGetValue(header, out var cid) || string.IsNullOrWhiteSpace(cid))
        cid = Guid.NewGuid().ToString("n");
    ctx.Items[header] = cid.ToString();
    ctx.Response.OnStarting(() =>
    {
        ctx.Response.Headers[header] = cid.ToString();
        return Task.CompletedTask;
    });
    await next();
});

app.UseExceptionHandler(); // surfaces ProblemDetails on exceptions

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");



app.Run();
