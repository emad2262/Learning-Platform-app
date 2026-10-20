using Microsoft.AspNetCore.Identity;

namespace Learning_Platform_app.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Academy> OwnedAcademies { get; set; } = new List<Academy>();
    public ICollection<Student> StudentProfiles { get; set; } = new List<Student>();
    public ICollection<Instructor> InstructorProfiles { get; set; } = new List<Instructor>();
}
