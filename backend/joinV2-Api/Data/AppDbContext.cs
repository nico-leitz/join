using Microsoft.EntityFrameworkCore;
using joinV2_Api.Models;

namespace joinV2_Api.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Contact> Contacts { get; set; }
  }
}
