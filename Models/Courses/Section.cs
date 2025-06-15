using AP_Project.Models.Classrooms;

namespace AP_Project.Models.Courses
{
    public class Section
    {
        public Guid Id { get; set; }
        public Guid? CourseId { get; set; }
        public int Semester { get; set; }
        public int Year { get; set; }
        public Guid? ClassroomId { get; set; }
        public int TimeSlotId { get; set; }

        public Course? Course { get; set; }
        public Classroom? Classroom { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public ICollection<Takes> Takes { get; set; }
        public ICollection<Teaches> Teaches { get; set; }
    }
}
