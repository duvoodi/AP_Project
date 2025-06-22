namespace AP_Project.Models.Courses
{
    public class Prerequisite
    {
        public Guid CourseId { get; set; }          
        public Guid PrerequisiteCourseCodeId { get; set; }

        public Course Course { get; set; }
        public CourseCode PrerequisiteCourseCode { get; set; }
    }
}
