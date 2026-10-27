using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;
using Microsoft.AspNetCore.Authorization;

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

    [HttpPost("guest-login")]
    public async Task<IActionResult> GuestLogin()
    {
      var guestEmail = "guest@join-app.com";
      var guestUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == guestEmail);

      if (guestUser == null)
      {
        guestUser = new User { Email = guestEmail, PasswordHash = BCrypt.Net.BCrypt.HashPassword("guest123!") };
        _context.Users.Add(guestUser);
        await _context.SaveChangesAsync();
      }

      var oldTasks = await _context.Tasks.Where(t => t.AuthUserId == guestUser.Id).ToListAsync();
      var oldTaskIds = oldTasks.Select(t => t.Id).ToList();

      var oldSubtasks = await _context.Subtasks.Where(s => oldTaskIds.Contains(s.TaskId)).ToListAsync();
      _context.Subtasks.RemoveRange(oldSubtasks);

      var oldAssignments = await _context.TaskAssignments.Where(ta => oldTaskIds.Contains(ta.TaskId)).ToListAsync();
      _context.TaskAssignments.RemoveRange(oldAssignments);

      _context.Tasks.RemoveRange(oldTasks);

      var oldContacts = await _context.Contacts.Where(c => c.AuthUserId == guestUser.Id).ToListAsync();
      _context.Contacts.RemoveRange(oldContacts);

      await _context.SaveChangesAsync();

      var contact1 = new Contact { FirstName = "Anton", LastName = "Test", Email = "anton@test.com", BadgeColor = "#ff7a00", AuthUserId = guestUser.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var contact2 = new Contact { FirstName = "Berta", LastName = "Example", Email = "berta@test.com", BadgeColor = "#9327ff", AuthUserId = guestUser.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var contact3 = new Contact { FirstName = "Christian", LastName = "Code", Email = "christian@test.com", BadgeColor = "#fc71ff", AuthUserId = guestUser.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      _context.Contacts.AddRange(contact1, contact2, contact3);

      await _context.SaveChangesAsync();

      var task1 = new TaskItem { Title = "Finish Backend API", Description = "Migrate all C# controllers and repositories.", Status = "todo", Priority = "urgent", Category = "Technical Task", SortOrder = 1, AuthUserId = guestUser.Id, DueDate = DateTime.UtcNow.AddDays(2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var task2 = new TaskItem { Title = "Test Angular Frontend", Description = "Check board and summary for missing data errors.", Status = "in-progress", Priority = "medium", Category = "User Story", SortOrder = 2, AuthUserId = guestUser.Id, DueDate = DateTime.UtcNow.AddDays(5), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var task3 = new TaskItem { Title = "Design Adjustments", Description = "Verify CSS variables for the Dark Theme.", Status = "await-feedback", Priority = "low", Category = "Design", SortOrder = 3, AuthUserId = guestUser.Id, DueDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var task4 = new TaskItem { Title = "Database Setup", Description = "Install and link PostgreSQL locally.", Status = "done", Priority = "urgent", Category = "Technical Task", SortOrder = 4, AuthUserId = guestUser.Id, DueDate = DateTime.UtcNow.AddDays(-2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      _context.Tasks.AddRange(task1, task2, task3, task4);

      await _context.SaveChangesAsync();

      var subtask1 = new Subtask { TaskId = task1.Id, Title = "Write Auth Controller", IsCompleted = true, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var subtask2 = new Subtask { TaskId = task1.Id, Title = "Implement JWT logic", IsCompleted = false, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var subtask3 = new Subtask { TaskId = task2.Id, Title = "Wire up Guest Login button", IsCompleted = false, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      _context.Subtasks.AddRange(subtask1, subtask2, subtask3);

      var assignment1 = new TaskAssignment { TaskId = task1.Id, ContactId = contact1.Id, CreatedAt = DateTime.UtcNow };
      var assignment2 = new TaskAssignment { TaskId = task1.Id, ContactId = contact2.Id, CreatedAt = DateTime.UtcNow };
      var assignment3 = new TaskAssignment { TaskId = task2.Id, ContactId = contact3.Id, CreatedAt = DateTime.UtcNow };
      var assignment4 = new TaskAssignment { TaskId = task3.Id, ContactId = contact1.Id, CreatedAt = DateTime.UtcNow };
      _context.TaskAssignments.AddRange(assignment1, assignment2, assignment3, assignment4);

      await _context.SaveChangesAsync();

      var token = GenerateJwtToken(guestUser);
      Response.Cookies.Append("jwt", token, new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
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
        isAnonymous = false
      });
    }
  }
}
