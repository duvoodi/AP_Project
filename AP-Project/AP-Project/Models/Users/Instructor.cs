using AP_Project.Models.Courses;

namespace AP_Project.Models.Users
{
    public class Instructor : User
    {
        public string InstructorId { get; set; } 
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }

        public ICollection<Teaches> Teaches { get; set; }
    }
}
