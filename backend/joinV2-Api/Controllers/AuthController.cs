using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;

namespace joinV2_Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
      _context = context;
      _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
      if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        return BadRequest(new { message = "Email is already taken." });

      var user = new User
      {
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
      };
      _context.Users.Add(user);

      var contact = new Contact
      {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        AuthUserId = user.Id,
        BadgeColor = "#000000",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };
      _context.Contacts.Add(contact);

      await _context.SaveChangesAsync();
      return Ok(new { message = "Registration successful." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
      if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Unauthorized(new { message = "Invalid email or password." });

      var token = GenerateJwtToken(user);

      Response.Cookies.Append("jwt", token, new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = DateTime.UtcNow.AddDays(7)
      });

      return Ok(new { message = "Login successful." });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
      Response.Cookies.Delete("jwt", new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None
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
  }
}
