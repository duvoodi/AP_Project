using AP_Project.Models.Classrooms;

namespace AP_Project.Models.Courses
{
    public class Section
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Semester { get; set; }
        public int Year { get; set; }
        public int ClassroomId { get; set; }
        public int TimeSlotId { get; set; }

        public Course Course { get; set; }
        public Classroom Classroom { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public ICollection<Takes> Takes { get; set; }
        public ICollection<Teaches> Teaches { get; set; }
    }
}
