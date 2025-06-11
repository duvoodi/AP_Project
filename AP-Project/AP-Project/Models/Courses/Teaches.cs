using AP_Project.Models.Users;

namespace AP_Project.Models.Courses
{
    public class Teaches
    {
        public int InstructorId { get; set; }
        public int SectionId { get; set; }

        public Instructor Instructor { get; set; }
        public Section Section { get; set; }
    }
}
