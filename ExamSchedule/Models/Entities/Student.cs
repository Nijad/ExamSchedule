namespace ExamSchedule.Models.Entities
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public List<int> CourseIds { get; set; }
    }
}
