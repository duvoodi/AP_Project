using AP_Project.Models.Users;

namespace AP_Project.Models.Courses
{
    public class Teaches
    {
        public Guid? InstructorUserId { get; set; }
        public Guid SectionId { get; set; }

        public Instructor? Instructor { get; set; }
        public Section Section { get; set; }
    }
}
