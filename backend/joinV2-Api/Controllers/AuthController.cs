using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;
using joinV2_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace joinV2_Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IGuestDataService _guestDataService;

    public AuthController(AppDbContext context, IConfiguration configuration, IGuestDataService guestDataService)
    {
      _context = context;
      _configuration = configuration;
      _guestDataService = guestDataService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
      var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

      if (normalizedEmail.StartsWith("guest_"))
        return BadRequest(new { message = "Email cannot start with 'guest_'." });

      if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
        return BadRequest(new { message = "Email is already taken." });

      var user = new User
      {
        Email = normalizedEmail,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
      };
      _context.Users.Add(user);

      var contact = new Contact
      {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = normalizedEmail,
        AuthUserId = user.Id,
        BadgeColor = "#000000",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };
      _context.Contacts.Add(contact);

      await _context.SaveChangesAsync();
      return Ok(new { message = "Registration successful." });
    }

    [EnableRateLimiting("AuthLimit")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
      var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

      if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Unauthorized(new { message = "Invalid email or password." });

      var token = GenerateJwtToken(user);

      Response.Cookies.Append("jwt", token, new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(7)
      });

      return Ok(new { message = "Login successful." });
    }

    [EnableRateLimiting("AuthLimit")]
    [HttpPost("guest-login")]
    public async Task<IActionResult> GuestLogin()
    {
      var guestId = Guid.NewGuid().ToString("N")[..8];
      var guestEmail = $"guest_{guestId}@join-app.com";
      var randomPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

      var guestUser = new User { Email = guestEmail, PasswordHash = randomPassword };
      _context.Users.Add(guestUser);
      await _context.SaveChangesAsync();

      await _guestDataService.GenerateDemoDataAsync(guestUser.Id);

      var token = GenerateJwtToken(guestUser);
      Response.Cookies.Append("jwt", token, new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(7)
      });

      return Ok(new { message = "Guest login successful." });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
      Response.Cookies.Delete("jwt", new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict
      });
      return Ok(new { message = "Logout successful." });
    }

    private string GenerateJwtToken(User user)
    {
      var jwtKey = _configuration["Jwt:Key"] ?? throw new Exception("JWT Key missing");
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
          new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
          new Claim(JwtRegisteredClaimNames.Email, user.Email)
      };

      var token = new JwtSecurityToken(
          claims: claims,
          expires: DateTime.UtcNow.AddDays(7),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);

      if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        return Unauthorized(new { message = "Invalid token claims." });

      var user = await _context.Users.FindAsync(userId);
      if (user == null)
        return Unauthorized(new { message = "User not found." });

      var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.AuthUserId == user.Id);
      var fullName = contact != null
          ? $"{contact.FirstName} {contact.LastName}".Trim()
          : user.Email.Split('@')[0];

      return Ok(new
      {
        id = user.Id.ToString(),
        email = user.Email,
        fullName = fullName,
        isAnonymous = user.Email.StartsWith("guest_")
      });
    }
  }
}
