using AP_Project.Models.Classrooms;

namespace AP_Project.Models.Courses
{
    public class Section
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string Semester { get; set; }
        public int Year { get; set; }
        public string ClassroomId { get; set; }
        public string TimeSlotId { get; set; }

        public Course Course { get; set; }
        public Classroom Classroom { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public ICollection<Takes> Takes { get; set; }
        public ICollection<Teaches> Teaches { get; set; }
    }
}
