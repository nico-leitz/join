using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace joinV2_Api.Controllers
{
  [Route("api/subtasks")]
  [ApiController]
  [Authorize]
  public class SubtasksController : ControllerBase
  {
    private readonly AppDbContext _context;

    public SubtasksController(AppDbContext context)
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
    public async Task<IActionResult> GetSubtasks([FromQuery] Guid? taskId)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var userTaskIds = await _context.Tasks.Where(t => t.AuthUserId == userId).Select(t => t.Id).ToListAsync();
      var query = _context.Subtasks.Where(s => userTaskIds.Contains(s.TaskId)).AsQueryable();

      if (taskId.HasValue)
      {
        query = query.Where(s => s.TaskId == taskId.Value);
      }

      var subtasks = await query
          .OrderBy(s => s.SortOrder)
          .Select(s => new { s.Id, s.TaskId, s.Title, s.IsCompleted, s.SortOrder })
          .ToListAsync();

      return Ok(subtasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubtask(CreateSubtaskDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var ownsTask = await _context.Tasks.AnyAsync(t => t.Id == dto.TaskId && t.AuthUserId == userId);
      if (!ownsTask) return NotFound(new { message = "Task not found or access denied." });

      var subtask = new Subtask
      {
        TaskId = dto.TaskId,
        Title = dto.Title,
        SortOrder = dto.SortOrder,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      _context.Subtasks.Add(subtask);
      await _context.SaveChangesAsync();

      return Ok(new { subtask.Id, subtask.TaskId, subtask.Title, subtask.IsCompleted, subtask.SortOrder });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubtask(Guid id, UpdateSubtaskDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var subtask = await _context.Subtasks.FirstOrDefaultAsync(s => s.Id == id);
      if (subtask == null) return NotFound();

      var ownsTask = await _context.Tasks.AnyAsync(t => t.Id == subtask.TaskId && t.AuthUserId == userId);
      if (!ownsTask) return NotFound();

      if (!string.IsNullOrWhiteSpace(dto.Title)) subtask.Title = dto.Title;
      if (dto.IsCompleted.HasValue) subtask.IsCompleted = dto.IsCompleted.Value;
      if (dto.SortOrder.HasValue) subtask.SortOrder = dto.SortOrder.Value;

      subtask.UpdatedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();

      return Ok(new { subtask.Id, subtask.TaskId, subtask.Title, subtask.IsCompleted, subtask.SortOrder });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubtask(Guid id)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var subtask = await _context.Subtasks.FirstOrDefaultAsync(s => s.Id == id);
      if (subtask == null) return NotFound();

      var ownsTask = await _context.Tasks.AnyAsync(t => t.Id == subtask.TaskId && t.AuthUserId == userId);
      if (!ownsTask) return NotFound();

      _context.Subtasks.Remove(subtask);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
