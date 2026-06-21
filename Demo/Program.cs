using System.Diagnostics;
using System.Text;
using Demo.Authorization;
using Demo.Infrastructure.ExceptionHandling;
using Demo.Middleware;
using Demo.Options;
using Demo.Services.Auth;
using Demo.Services.Students;
using Demo.Services.Tokens;
using Demo.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddTransient<RequestContextMiddleware>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<RefreshTokenCookieOptions>(builder.Configuration.GetSection(RefreshTokenCookieOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

if (jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppPolicies.CanReadStudents, policy => policy.RequireRole(
        AppRoles.Admin,
        AppRoles.StudentReader,
        AppRoles.StudentWriter))
    .AddPolicy(AppPolicies.CanManageStudents, policy => policy.RequireRole(
        AppRoles.Admin,
        AppRoles.StudentWriter));

builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<IDemoUserStore, DemoUserStore>();
builder.Services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
builder.Services.AddSingleton<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Demo API",
        Version = "v1",
        Description = "Demo .NET 10 Web API."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your access token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    };

    var securitySchemeReference = new OpenApiSecuritySchemeReference(
        JwtBearerDefaults.AuthenticationScheme,
        hostDocument: null!,
        externalResource: null!);

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [securitySchemeReference] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<RequestContextMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var swaggerUrl = app.Urls
            .OrderByDescending(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .Select(url => $"{url.TrimEnd('/')}/swagger")
            .FirstOrDefault();

        if (swaggerUrl is null)
        {
            return;
        }

        try
        {
            OpenBrowser(swaggerUrl);
        }
        catch (Exception exception)
        {
            app.Logger.LogWarning(exception, "Could not open Swagger automatically at {SwaggerUrl}", swaggerUrl);
        }
    });
}

app.Run();

static void OpenBrowser(string url)
{
    if (OperatingSystem.IsMacOS())
    {
        Process.Start("open", url);
        return;
    }

    if (OperatingSystem.IsWindows())
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
        return;
    }

    if (OperatingSystem.IsLinux())
    {
        Process.Start("xdg-open", url);
    }
}
