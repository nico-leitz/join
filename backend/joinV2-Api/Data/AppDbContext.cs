using joinV2_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace joinV2_Api.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Subtask> Subtasks { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<TaskAssignment>()
          .HasKey(ta => new { ta.TaskId, ta.ContactId });
    }
  }
}
