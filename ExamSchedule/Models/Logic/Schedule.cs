using SchedualTest.Models.Entities;

namespace SchedualTest.Models.Logic
{
    public class Schedule
    {
        public List<Exam> Exams { get; set; }
        public int Conflict { get; set; }
        public double Fitness
        {
            get
            {
                return 1 / (Conflict + 1);
            }
        }

        public void CalculateConflict()
        {
            //room-time slot conflict
            Conflict += Exams.Count - Exams.GroupBy(x => new { x.Room, x.TimeSlot }).Count();

            //student-time slot conflict
            Dictionary<Student, List<TimeSlot>> studentSlots = new Dictionary<Student, List<TimeSlot>>();
            foreach (Exam exam in Exams)
                foreach (Student student in exam.Students)
                {
                    if (!studentSlots.ContainsKey(student))
                        studentSlots.Add(student, new List<TimeSlot>());
                    studentSlots[student].Add(exam.TimeSlot);
                }

            foreach (Student student in studentSlots.Keys)
                Conflict += studentSlots[student].Count - studentSlots[student].Distinct().Count();
        }
    }
}
