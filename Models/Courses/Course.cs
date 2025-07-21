namespace AP_Project.Models.Courses
{
    public class Course
    {
        public Guid Id { get; set; }
        public Guid CodeId { get; set; }
        public int Unit { get; set; }
        public string Description { get; set; }
        public DateTime FinalExamDate { get; set; }

        public CourseCode CourseCode { get; set; }
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<Prerequisite> Prerequisites { get; set; } = new List<Prerequisite>();
    }
}
