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

    private Guid? GetUserId()
    {
      var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
      if (claim == null || !Guid.TryParse(claim.Value, out var parsedGuid)) return null;

      return parsedGuid;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var tasks = await _context.Tasks
          .Where(t => t.AuthUserId == userId)
          .Select(t => new
          {
            t.Id,
            t.Title,
            t.Description,
            t.DueDate,
            t.Priority,
            t.Category,
            t.Status,
            t.SortOrder
          })
          .ToListAsync();

      return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var task = await _context.Tasks
          .Where(t => t.Id == id && t.AuthUserId == userId)
          .Select(t => new
          {
            t.Id,
            t.Title,
            t.Description,
            t.DueDate,
            t.Priority,
            t.Category,
            t.Status,
            t.SortOrder
          })
          .FirstOrDefaultAsync();

      if (task == null) return NotFound();
      return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      DateTime utcDueDate = dto.DueDate.HasValue
          ? (dto.DueDate.Value.Kind == DateTimeKind.Unspecified
              ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
              : dto.DueDate.Value.ToUniversalTime())
          : DateTime.UtcNow;

      var task = new TaskItem
      {
        AuthUserId = userId.Value,
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

      return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, new
      {
        task.Id,
        task.Title,
        task.Description,
        task.DueDate,
        task.Priority,
        task.Category,
        task.Status,
        task.SortOrder
      });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskDto dto)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

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

      return Ok(new
      {
        existingTask.Id,
        existingTask.Title,
        existingTask.Description,
        existingTask.DueDate,
        existingTask.Priority,
        existingTask.Category,
        existingTask.Status,
        existingTask.SortOrder
      });
    }

    [HttpGet("{id}/contacts")]
    public async Task<IActionResult> GetContactsForTask(Guid id)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var taskExists = await _context.Tasks.AnyAsync(t => t.Id == id && t.AuthUserId == userId);
      if (!taskExists) return NotFound();

      var contactIds = await _context.TaskAssignments
          .Where(ta => ta.TaskId == id)
          .Select(ta => ta.ContactId)
          .ToListAsync();

      var contacts = await _context.Contacts
          .Where(c => contactIds.Contains(c.Id) && c.AuthUserId == userId)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
      var userId = GetUserId();
      if (userId == null) return Unauthorized(new { message = "Invalid token claims." });

      var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.AuthUserId == userId);
      if (task == null) return NotFound();

      _context.Tasks.Remove(task);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
