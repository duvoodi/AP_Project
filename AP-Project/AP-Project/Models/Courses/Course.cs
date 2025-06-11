namespace AP_Project.Models.Courses
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public DateTime FinalExamDate { get; set; }

        public ICollection<Section> Sections { get; set; }
        public ICollection<Prerequisite> Prerequisites { get; set; }
    }
}
