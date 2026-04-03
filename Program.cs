using HTruyen.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient("OTruyenClient", client => {
    client.BaseAddress = new Uri("https://otruyenapi.com/v1/api/");
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
});

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "DefaultSecretKeyNeedToBeLongEnough123!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Configure CORS for Next.js
builder.Services.AddCors(options =>
{
    var frontendUrl = (builder.Configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
    
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => 
                  {
                      if (string.IsNullOrWhiteSpace(origin)) return false;
                      var normalizedOrigin = origin.TrimEnd('/');
                      return normalizedOrigin == frontendUrl || 
                             normalizedOrigin == "http://localhost:3000" ||
                             (normalizedOrigin.Contains("vercel.app") && normalizedOrigin.Contains("h-truyen-fe"));
                  })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
else 
{
    // Ensure forwarded headers are handled correctly behind Render's proxy
    app.UseForwardedHeaders();
}

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
