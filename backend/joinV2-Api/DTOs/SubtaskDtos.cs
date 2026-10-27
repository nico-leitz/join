namespace joinV2_Api.DTOs
{
  public class CreateSubtaskDto
  {
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
  }

  public class UpdateSubtaskDto
  {
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
    public int? SortOrder { get; set; }
  }
}
