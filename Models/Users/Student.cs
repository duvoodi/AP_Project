using AP_Project.Models.Courses;

namespace AP_Project.Models.Users
{
    public class Student : User
    {
        public int StudentId { get; set; } 
        public int EnrollmentYear { get; set; }

        public ICollection<Takes> Takes { get; set; }
    }
}
