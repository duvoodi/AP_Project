namespace AP_Project.Models.Courses
{
    public class Prerequisite
    {
        public int CourseId { get; set; }          
        public int PrerequisiteCourseId { get; set; }

        public Course Course { get; set; }
        public Course PrerequisiteCourse { get; set; }
    }
}
