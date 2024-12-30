namespace SchedualTest.Models.Entities
{
    public class Exam
    {
        public int ExamId { get; set; }
        public Course Course { get; set; }
        public Room Room { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }
}
