using Microsoft.EntityFrameworkCore;
using joinV2_Api.Data;
using joinV2_Api.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
  throw new Exception("Key wasnt found in appsettings");
}
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
    ValidateIssuer = false,
    ValidateAudience = false,
    ClockSkew = TimeSpan.Zero
  };

  options.Events = new JwtBearerEvents
  {
    OnMessageReceived = context =>
    {
      var token = context.Request.Cookies["jwt"];
      if (!string.IsNullOrEmpty(token))
      {
        context.Token = token;
      }
      return Task.CompletedTask;
    }
  };
});

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAngular", policy =>
  {
 
    policy.WithOrigins("http://localhost:4200", "https://dein-projekt.de")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
  });
});


builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
  options.AddFixedWindowLimiter("AuthLimit", opt =>
  {
    opt.Window = TimeSpan.FromMinutes(1);
    opt.PermitLimit = 5;
    opt.QueueLimit = 0;
  });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IGuestDataService, GuestDataService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
