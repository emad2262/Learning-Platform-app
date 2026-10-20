namespace Learning_Platform_app.Domain.Entities;

public class Academy :BaseEntity
{
  
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public string OwnerUserId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
}
