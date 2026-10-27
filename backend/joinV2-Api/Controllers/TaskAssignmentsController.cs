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

    private Guid GetUserId()
    {
      var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
      return Guid.Parse(claim!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskAssignment>>> GetAssignments([FromQuery] Guid? taskId)
    {
      var userId = GetUserId();
      var userTaskIds = await _context.Tasks.Where(t => t.AuthUserId == userId).Select(t => t.Id).ToListAsync();

      var query = _context.TaskAssignments.Where(ta => userTaskIds.Contains(ta.TaskId)).AsQueryable();

      if (taskId.HasValue)
      {
        query = query.Where(ta => ta.TaskId == taskId.Value);
      }

      return await query.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<TaskAssignment>> CreateAssignment(CreateTaskAssignmentDto dto)
    {
      var userId = GetUserId();

      var taskExists = await _context.Tasks.AnyAsync(t => t.Id == dto.TaskId && t.AuthUserId == userId);
      if (!taskExists) return Forbid();

      var existingAssignment = await _context.TaskAssignments
          .FirstOrDefaultAsync(ta => ta.TaskId == dto.TaskId && ta.ContactId == dto.ContactId);

      if (existingAssignment != null)
      {
        return Ok(existingAssignment);
      }

      var assignment = new TaskAssignment
      {
        TaskId = dto.TaskId,
        ContactId = dto.ContactId,
        CreatedAt = DateTime.UtcNow
      };

      _context.TaskAssignments.Add(assignment);
      await _context.SaveChangesAsync();

      return Ok(assignment);
    }

    [HttpDelete("{taskId}/{contactId}")]
    public async Task<IActionResult> DeleteAssignment(Guid taskId, Guid contactId)
    {
      var userId = GetUserId();

      var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId && t.AuthUserId == userId);
      if (!taskExists) return Forbid();

      var assignment = await _context.TaskAssignments.FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.ContactId == contactId);
      if (assignment == null) return NotFound();

      _context.TaskAssignments.Remove(assignment);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
