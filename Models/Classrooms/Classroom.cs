using AP_Project.Models.Courses;

namespace AP_Project.Models.Classrooms
{
    public class Classroom
    {
        public Guid Id { get; set; }
        public string Building { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }

        public ICollection<Section> Sections { get; set; }
    }
}
