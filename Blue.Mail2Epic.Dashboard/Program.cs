using System.Text;
using Blue.Mail2Epic.Dashboard.Models;
using Blue.Mail2Epic.Dashboard.Services;
using Blue.Mail2Epic.Infrastructure;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models.Configuration;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DataProtectionOptions = Blue.Mail2Epic.Infrastructure.Models.Configuration.DataProtectionOptions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionString"];
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString, o => o.UseVector()));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the field below. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.Configure<GoogleOAuthOptions>(builder.Configuration.GetSection(GoogleOAuthOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection(FrontendOptions.SectionName));
builder.Services.Configure<JiraOptions>(builder.Configuration.GetSection(JiraOptions.SectionName));

var dataProtectionOptions = builder.Configuration
    .GetSection(DataProtectionOptions.SectionName)
    .Get<DataProtectionOptions>()!;

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionOptions.KeyStoragePath))
    .SetApplicationName(Application.Name);
builder.Services.AddScoped<ISecretProtector, SecretProtector>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LoginExchangeStore>();

builder.Services.AddHttpClient(JiraService.HttpClientName);
builder.Services.AddTransient<IJiraService, JiraService>();

builder.Services.AddScoped<IGoogleTokenService, GoogleTokenService>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<HistoryRowMapper>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidateIssuer = true,
            ValidAudience = jwtSettings.Audience,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
