using AP_Project.Models.Courses;

namespace AP_Project.Models.Users
{
    public class Instructor : User
    {
        public int InstructorId { get; set; } 
        public decimal Salary { get; set; }
        public int HireYear { get; set; }

        public ICollection<Teaches> Teaches { get; set; } = new List<Teaches>();
    }
}
