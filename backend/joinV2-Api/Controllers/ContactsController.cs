using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace joinV2_Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class ContactsController : ControllerBase
  {
    private readonly AppDbContext _context;

    public ContactsController(AppDbContext context)
    {
      _context = context;
    }

    private Guid? GetUserId()
    {
      var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
      if (claim == null || !Guid.TryParse(claim.Value, out var parsedGuid))
      {
        return null;
      }
      return parsedGuid;
    }

    [HttpGet]
    public async Task<IActionResult> GetContacts()
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var contacts = await _context.Contacts
          .Where(c => c.AuthUserId == userId)
          .Select(c => new
          {
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.Phone,
            c.BadgeColor
          })
          .ToListAsync();

      return Ok(contacts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContactById(Guid id)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var contact = await _context.Contacts
          .Where(c => c.Id == id && c.AuthUserId == userId)
          .Select(c => new
          {
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.Phone,
            c.BadgeColor
          })
          .FirstOrDefaultAsync();

      if (contact == null) return NotFound();
      return Ok(contact);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContact(CreateContactDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var contact = new Contact
      {
        AuthUserId = userId.Value,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Phone = dto.Phone,
        BadgeColor = dto.BadgeColor,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      _context.Contacts.Add(contact);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, new
      {
        contact.Id,
        contact.FirstName,
        contact.LastName,
        contact.Email,
        contact.Phone,
        contact.BadgeColor
      });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(Guid id, UpdateContactDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AuthUserId == userId);

      if (contact == null) return NotFound();

      contact.FirstName = dto.FirstName;
      contact.LastName = dto.LastName;
      contact.Email = dto.Email;
      contact.Phone = dto.Phone;
      contact.BadgeColor = dto.BadgeColor;
      contact.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Ok(new
      {
        contact.Id,
        contact.FirstName,
        contact.LastName,
        contact.Email,
        contact.Phone,
        contact.BadgeColor
      });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AuthUserId == userId);

      if (contact == null) return NotFound();

      _context.Contacts.Remove(contact);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
