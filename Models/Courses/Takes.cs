using AP_Project.Models.Users;

namespace AP_Project.Models.Courses
{
    public class Takes
    {
        public int StudentId { get; set; }
        public int SectionId { get; set; }
        public string Grade { get; set; }

        public Student Student { get; set; }
        public Section Section { get; set; }
    }
}
