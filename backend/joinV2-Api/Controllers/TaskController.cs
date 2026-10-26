using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using joinV2_Api.Data;
using joinV2_Api.Models;
using joinV2_Api.DTOs;

namespace joinV2_Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TasksController : ControllerBase
  {
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
      return await _context.Tasks.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskById(Guid id)
    {
      var task = await _context.Tasks.FindAsync(id);

      if (task == null)
      {
        return NotFound();
      }

      return task;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskDto dto)
    {
      var task = new TaskItem
      {
        Title = dto.Title,
        Description = dto.Description,
        DueDate = dto.DueDate,
        Priority = dto.Priority,
        Category = dto.Category,
        Status = "To Do",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      _context.Tasks.Add(task);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskDto dto)
    {
      var existingTask = await _context.Tasks.FindAsync(id);
      if (existingTask == null) return NotFound();

      existingTask.Title = dto.Title;
      existingTask.Description = dto.Description;
      existingTask.DueDate = dto.DueDate;
      existingTask.Priority = dto.Priority;
      existingTask.Category = dto.Category;
      existingTask.Status = dto.Status;
      existingTask.SortOrder = dto.SortOrder;
      existingTask.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
      var task = await _context.Tasks.FindAsync(id);
      if (task == null) return NotFound();

      _context.Tasks.Remove(task);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
