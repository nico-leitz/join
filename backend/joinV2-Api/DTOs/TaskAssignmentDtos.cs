namespace joinV2_Api.DTOs
{
  public class CreateTaskAssignmentDto
  {
    public Guid TaskId { get; set; }
    public Guid ContactId { get; set; }
  }
}
