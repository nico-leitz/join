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
  public class SubtasksController : ControllerBase
  {
    private readonly AppDbContext _context;

    public SubtasksController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<IEnumerable<Subtask>>> GetSubtasksForTask(Guid taskId)
    {
      return await _context.Subtasks
          .Where(s => s.TaskId == taskId)
          .OrderBy(s => s.SortOrder)
          .ToListAsync();
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

      subtask.Title = dto.Title;
      subtask.IsCompleted = dto.IsCompleted;
      subtask.SortOrder = dto.SortOrder;
      subtask.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
      return NoContent();
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
