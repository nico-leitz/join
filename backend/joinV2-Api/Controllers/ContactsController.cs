using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
    {
      return await _context.Contacts.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Contact>> GetContactById(Guid id)
    {
      var contact = await _context.Contacts.FindAsync(id);
      if (contact == null) return NotFound();
      return contact;
    }

    [HttpPost]
    public async Task<ActionResult<Contact>> CreateContact(CreateContactDto dto)
    {
      var contact = new Contact
      {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Phone = dto.Phone,
        BadgeColor = dto.BadgeColor,
        AuthUserId = dto.AuthUserId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      _context.Contacts.Add(contact);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, contact);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(Guid id, UpdateContactDto dto)
    {
      var contact = await _context.Contacts.FindAsync(id);
      if (contact == null) return NotFound();

      contact.FirstName = dto.FirstName;
      contact.LastName = dto.LastName;
      contact.Email = dto.Email;
      contact.Phone = dto.Phone;
      contact.BadgeColor = dto.BadgeColor;
      contact.AuthUserId = dto.AuthUserId;
      contact.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
      var contact = await _context.Contacts.FindAsync(id);
      if (contact == null) return NotFound();

      _context.Contacts.Remove(contact);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
