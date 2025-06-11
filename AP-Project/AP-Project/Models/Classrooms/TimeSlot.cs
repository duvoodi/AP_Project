using AP_Project.Models.Courses;

namespace AP_Project.Models.Classrooms
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public string Day { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ICollection<Section> Sections { get; set; }
    }
}
