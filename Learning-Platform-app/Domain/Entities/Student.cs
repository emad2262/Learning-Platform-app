namespace Learning_Platform_app.Domain.Entities;

public class Student :BaseEntity
{
    public int AcademyId { get; set; }
    public Academy Academy { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string? SchoolName { get; set; }
    public int Points { get; set; }
    public bool IsActive { get; set; } = true;
}
