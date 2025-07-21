namespace AP_Project.Models.Courses
{
    public class CourseCode
    {
        public Guid Id { get; set; }  
        public int Code { get; set; }   
        public string Title { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}