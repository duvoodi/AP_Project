namespace AP_Project.Models.Courses
{
    public class Prerequisite
    {
        public string CourseId { get; set; }          
        public string PrerequisiteCourseId { get; set; }

        public Course Course { get; set; }
        public Course PrerequisiteCourse { get; set; }
    }
}
