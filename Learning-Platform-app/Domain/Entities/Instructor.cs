namespace Learning_Platform_app.Domain.Entities;

public class Instructor : BaseEntity
{
    public string? Specialization { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int AcademyId { get; set; }
    public Academy Academy { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
