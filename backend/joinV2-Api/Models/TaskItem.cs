using System;
using System.ComponentModel.DataAnnotations;

namespace joinV2_Api.Models
{
  public class TaskItem
  {
    [Key]
    public Guid Id { get; set; }
    public Guid AuthUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = "medium";
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "todo";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}
