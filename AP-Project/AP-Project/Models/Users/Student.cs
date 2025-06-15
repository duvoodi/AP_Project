using AP_Project.Models.Courses;

namespace AP_Project.Models.Users
{
    public class Student : User
    {
        public string StudentId { get; set; } 
        public DateTime EnrollmentDate { get; set; }

        public ICollection<Takes> Takes { get; set; }
    }
}
