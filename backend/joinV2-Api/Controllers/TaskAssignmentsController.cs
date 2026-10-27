using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace joinV2_Api.Controllers
{
  [Route("api/task-assignments")]
  [ApiController]
  [Authorize]
  public class TaskAssignmentsController : ControllerBase
  {
    private readonly AppDbContext _context;

    public TaskAssignmentsController(AppDbContext context)
    {
      _context = context;
    }

    private Guid? GetUserId()
    {
      var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
      if (claim == null || !Guid.TryParse(claim.Value, out var parsedGuid)) return null;

      return parsedGuid;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignments([FromQuery] Guid? taskId)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var userTaskIds = await _context.Tasks.Where(t => t.AuthUserId == userId).Select(t => t.Id).ToListAsync();
      var query = _context.TaskAssignments.Where(ta => userTaskIds.Contains(ta.TaskId)).AsQueryable();

      if (taskId.HasValue)
      {
        query = query.Where(ta => ta.TaskId == taskId.Value);
      }

      var assignments = await query
          .Select(ta => new { ta.TaskId, ta.ContactId })
          .ToListAsync();

      return Ok(assignments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssignment(CreateTaskAssignmentDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var taskExists = await _context.Tasks.AnyAsync(t => t.Id == dto.TaskId && t.AuthUserId == userId);
      if (!taskExists) return NotFound(new { message = "Task not found or access denied." });

      var contactExists = await _context.Contacts.AnyAsync(c => c.Id == dto.ContactId && c.AuthUserId == userId);
      if (!contactExists) return NotFound(new { message = "Contact not found or access denied." });

      var existingAssignment = await _context.TaskAssignments
          .FirstOrDefaultAsync(ta => ta.TaskId == dto.TaskId && ta.ContactId == dto.ContactId);

      if (existingAssignment != null)
      {
        return Ok(new { existingAssignment.TaskId, existingAssignment.ContactId });
      }

      var assignment = new TaskAssignment
      {
        TaskId = dto.TaskId,
        ContactId = dto.ContactId,
        CreatedAt = DateTime.UtcNow
      };

      _context.TaskAssignments.Add(assignment);
      await _context.SaveChangesAsync();

      return Ok(new { assignment.TaskId, assignment.ContactId });
    }

    [HttpDelete("{taskId}/{contactId}")]
    public async Task<IActionResult> DeleteAssignment(Guid taskId, Guid contactId)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId && t.AuthUserId == userId);
      if (!taskExists) return NotFound(new { message = "Task not found or access denied." });

      var assignment = await _context.TaskAssignments.FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.ContactId == contactId);
      if (assignment == null) return NotFound();

      _context.TaskAssignments.Remove(assignment);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
