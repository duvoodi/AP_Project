using AP_Project.Models.Users;

namespace AP_Project.Models.Courses
{
    public class Takes
    {
        public string StudentId { get; set; }
        public string SectionId { get; set; }
        public string Grade { get; set; }

        public Student Student { get; set; }
        public Section Section { get; set; }
    }
}
