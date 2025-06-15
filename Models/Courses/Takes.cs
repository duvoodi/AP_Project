using AP_Project.Models.Users;

namespace AP_Project.Models.Courses
{
    public class Takes
    {
        public Guid StudentUserId { get; set; }
        public Guid SectionId { get; set; }
        public string Grade { get; set; }

        public Student Student { get; set; }
        public Section Section { get; set; }
    }
}
