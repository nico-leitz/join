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

    private Guid GetUserId()
    {
      var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
      return Guid.Parse(claim!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Subtask>>> GetSubtasks([FromQuery] Guid? taskId)
    {
      var userId = GetUserId();
      var userTaskIds = await _context.Tasks.Where(t => t.AuthUserId == userId).Select(t => t.Id).ToListAsync();

      var query = _context.Subtasks.Where(s => userTaskIds.Contains(s.TaskId)).AsQueryable();

      if (taskId.HasValue)
      {
        query = query.Where(s => s.TaskId == taskId.Value);
      }

      return await query.OrderBy(s => s.SortOrder).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Subtask>> CreateSubtask(CreateSubtaskDto dto)
    {
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

      return Ok(subtask);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubtask(Guid id, UpdateSubtaskDto dto)
    {
      var subtask = await _context.Subtasks.FindAsync(id);
      if (subtask == null) return NotFound();

      if (!string.IsNullOrWhiteSpace(dto.Title)) subtask.Title = dto.Title;
      if (dto.IsCompleted.HasValue) subtask.IsCompleted = dto.IsCompleted.Value;
      if (dto.SortOrder.HasValue) subtask.SortOrder = dto.SortOrder.Value;

      subtask.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Ok(subtask);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubtask(Guid id)
    {
      var subtask = await _context.Subtasks.FindAsync(id);
      if (subtask == null) return NotFound();

      _context.Subtasks.Remove(subtask);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
