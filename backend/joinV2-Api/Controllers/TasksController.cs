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
  public class TasksController : ControllerBase
  {
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
      _context = context;
    }

    private Guid GetUserId()
    {
      var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
      return Guid.Parse(claim!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
      var userId = GetUserId();
      return await _context.Tasks.Where(t => t.AuthUserId == userId).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskById(Guid id)
    {
      var userId = GetUserId();
      var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.AuthUserId == userId);

      if (task == null) return NotFound();
      return task;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskDto dto)
    {
      DateTime utcDueDate = dto.DueDate.HasValue
          ? (dto.DueDate.Value.Kind == DateTimeKind.Unspecified
              ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
              : dto.DueDate.Value.ToUniversalTime())
          : DateTime.UtcNow;

      var task = new TaskItem
      {
        AuthUserId = GetUserId(),
        Title = dto.Title,
        Description = dto.Description ?? "",
        DueDate = utcDueDate,
        Priority = dto.Priority ?? "medium",
        Category = dto.Category ?? "user_story",
        Status = "todo",
        SortOrder = 1,
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
      var userId = GetUserId();
      var existingTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.AuthUserId == userId);

      if (existingTask == null) return NotFound();

      if (!string.IsNullOrWhiteSpace(dto.Title)) existingTask.Title = dto.Title;
      if (dto.Description != null) existingTask.Description = dto.Description;
      if (!string.IsNullOrWhiteSpace(dto.Priority)) existingTask.Priority = dto.Priority;
      if (!string.IsNullOrWhiteSpace(dto.Category)) existingTask.Category = dto.Category;
      if (!string.IsNullOrWhiteSpace(dto.Status)) existingTask.Status = dto.Status;

      if (dto.SortOrder.HasValue) existingTask.SortOrder = dto.SortOrder.Value;

      if (dto.DueDate.HasValue)
      {
        existingTask.DueDate = dto.DueDate.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
            : dto.DueDate.Value.ToUniversalTime();
      }

      existingTask.UpdatedAt = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Ok(existingTask);
    }

    [HttpGet("{id}/contacts")]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContactsForTask(Guid id)
    {
      var userId = GetUserId();
      var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.AuthUserId == userId);

      if (task == null) return NotFound();

      var contactIds = await _context.TaskAssignments
          .Where(ta => ta.TaskId == id)
          .Select(ta => ta.ContactId)
          .ToListAsync();

      var contacts = await _context.Contacts
          .Where(c => contactIds.Contains(c.Id))
          .ToListAsync();

      return Ok(contacts);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
      var userId = GetUserId();
      var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.AuthUserId == userId);

      if (task == null) return NotFound();

      _context.Tasks.Remove(task);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
