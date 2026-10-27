namespace joinV2_Api.DTOs
{
  public class CreateContactDto
  {
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string BadgeColor { get; set; } = string.Empty;
    public Guid? AuthUserId { get; set; }
  }

  public class UpdateContactDto
  {
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string BadgeColor { get; set; } = string.Empty;
    public Guid? AuthUserId { get; set; }
  }
}
