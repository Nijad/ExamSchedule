using ExamSchedule.Models.Entities;

namespace ExamSchedule.Models.Logic
{
    public class GAResult
    {
        public int GenerationCount { get; set; }
        public Schedule BestSchedule { get; set; }
        public Dictionary<Student,List<Exam>> StudentsExmas { get; set; }
        public bool HasError { get; set; } = false;
        public string ErrorMessage { get; set; }
    }
}
