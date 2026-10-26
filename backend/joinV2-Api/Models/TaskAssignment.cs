namespace joinV2_Api.Models
{
  public class TaskAssignment
  {
    public Guid TaskId { get; set; }
    public Guid ContactId { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
