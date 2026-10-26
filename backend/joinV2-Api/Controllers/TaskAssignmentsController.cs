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
  public class TaskAssignmentsController : ControllerBase
  {
    private readonly AppDbContext _context;

    public TaskAssignmentsController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<IEnumerable<TaskAssignment>>> GetAssignmentsForTask(Guid taskId)
    {
      return await _context.TaskAssignments
          .Where(ta => ta.TaskId == taskId)
          .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<TaskAssignment>> CreateAssignment(CreateTaskAssignmentDto dto)
    {
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
      var assignment = await _context.TaskAssignments.FindAsync(taskId, contactId);
      if (assignment == null) return NotFound();

      _context.TaskAssignments.Remove(assignment);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
