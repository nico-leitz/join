namespace joinV2_Api.DTOs
{
  public class CreateTaskDto
  {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = "To Do";
  }

    public class UpdateTaskDto
    {
      public string? Title { get; set; }
      public string? Description { get; set; }
      public DateTime? DueDate { get; set; }
      public string? Priority { get; set; }
      public string? Category { get; set; }
      public string? Status { get; set; }
      public int? SortOrder { get; set; }
    }
}
