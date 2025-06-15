using AP_Project.Models.Courses;

namespace AP_Project.Models.Classrooms
{
    public class TimeSlot
    {
        public string Id { get; set; }
        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public ICollection<Section> Sections { get; set; }
    }
}
