using joinV2_Api.Data;
using joinV2_Api.Models;

namespace joinV2_Api.Services
{
  public interface IGuestDataService
  {
    Task GenerateDemoDataAsync(Guid guestUserId);
  }

  public class GuestDataService : IGuestDataService
  {
    private readonly AppDbContext _context;

    public GuestDataService(AppDbContext context)
    {
      _context = context;
    }

    public async Task GenerateDemoDataAsync(Guid guestUserId)
    {
      var contact1 = new Contact { FirstName = "Anton", LastName = "Test", Email = "anton@test.com", BadgeColor = "#ff7a00", AuthUserId = guestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var contact2 = new Contact { FirstName = "Berta", LastName = "Example", Email = "berta@test.com", BadgeColor = "#9327ff", AuthUserId = guestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var contact3 = new Contact { FirstName = "Christian", LastName = "Code", Email = "christian@test.com", BadgeColor = "#fc71ff", AuthUserId = guestUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      _context.Contacts.AddRange(contact1, contact2, contact3);
      await _context.SaveChangesAsync();

      var task1 = new TaskItem { Title = "Finish Backend API", Description = "Migrate all C# controllers.", Status = "todo", Priority = "urgent", Category = "Technical Task", SortOrder = 1, AuthUserId = guestUserId, DueDate = DateTime.UtcNow.AddDays(2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var task2 = new TaskItem { Title = "Test Angular Frontend", Description = "Check board and summary.", Status = "in-progress", Priority = "medium", Category = "User Story", SortOrder = 2, AuthUserId = guestUserId, DueDate = DateTime.UtcNow.AddDays(5), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var task3 = new TaskItem { Title = "Design Adjustments", Description = "Verify CSS variables.", Status = "await-feedback", Priority = "low", Category = "Design", SortOrder = 3, AuthUserId = guestUserId, DueDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      _context.Tasks.AddRange(task1, task2, task3);
      await _context.SaveChangesAsync();

      var subtask1 = new Subtask { TaskId = task1.Id, Title = "Write Auth Controller", IsCompleted = true, SortOrder = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      var subtask2 = new Subtask { TaskId = task1.Id, Title = "Implement JWT logic", IsCompleted = false, SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
      _context.Subtasks.AddRange(subtask1, subtask2);

      var assignment1 = new TaskAssignment { TaskId = task1.Id, ContactId = contact1.Id, CreatedAt = DateTime.UtcNow };
      var assignment2 = new TaskAssignment { TaskId = task1.Id, ContactId = contact2.Id, CreatedAt = DateTime.UtcNow };
      var assignment3 = new TaskAssignment { TaskId = task2.Id, ContactId = contact3.Id, CreatedAt = DateTime.UtcNow };
      _context.TaskAssignments.AddRange(assignment1, assignment2, assignment3);

      await _context.SaveChangesAsync();
    }
  }
}
