using ExamSchedule.Models.Entities;

namespace ExamSchedule.Models.Logic
{
    public class ExamScheduler
    {
        private List<Course> Courses;
        private List<Exam> Exams = new List<Exam>();
        private List<Student> Students;
        private List<Room> Rooms;
        private List<TimeSlot> TimeSlots;
        Random rand = new Random();
        public ExamScheduler(IFormFile courseFile,
                             IFormFile studentsFile,
                             IFormFile roomsFile,
                             IFormFile timeSlotsFile)
        {
            Courses = LoadCourses(courseFile);
            Students = LoadStudents(studentsFile);
            Rooms = LoadRooms(roomsFile);
            TimeSlots = LoadTimeSlots(timeSlotsFile);
        }

        public ExamScheduler(IFormFile courseFile,
                             IFormFile studentsFile,
                             IFormFile roomsFile,
                             double examDuration,
                             double gap,
                             DateTime startDate,
                             DateTime lastDate,
                             int examPerDay,
                             int[] holidays)
        {
            Courses = LoadCourses(courseFile);
            Students = LoadStudents(studentsFile);
            Rooms = LoadRooms(roomsFile);
            TimeSlots = new List<TimeSlot>();
            //calculate time slots
            DateTime day = startDate;
            int slotId = 0;
            while (day <= lastDate)
            {
                if (holidays.Contains((int)day.DayOfWeek))
                {
                    day = day.AddDays(1);
                    continue;
                }
                double h = Math.Floor(examDuration + gap);
                double m = (examDuration + gap - h) * 60;
                DateTime t = new DateTime(day.Year, day.Month, day.Day, 9, 0, 0);
                for (int i = 0; i < examPerDay; i++)
                {
                    TimeSlot timeSlot = new TimeSlot();
                    timeSlot.SlotId = slotId;
                    timeSlot.Duration = examDuration;
                    timeSlot.StartTime = t;
                    TimeSlots.Add(timeSlot);
                    slotId++;
                    if (i < examPerDay - 1)
                    {
                        t = t.AddHours(h);
                        t = t.AddMinutes(m);
                    }
                }
                day = day.AddDays(1);
            }
        }

        private List<Course> LoadCourses(IFormFile file)
        {
            List<Course> courses = new List<Course>();
            string filePath = Path.GetTempFileName();

            using (var stream = File.Create(filePath))
            {
                file.CopyTo(stream);
            }
            foreach (string? line in File.ReadLines(filePath).Skip(1)) // Skip header
            {
                string[] columns = line.Split(',');
                Course course = new Course
                {
                    CourseId = int.Parse(columns[0]),
                    CourseName = columns[1],
                    ExamDuration = double.Parse(columns[2])
                };
                courses.Add(course);
            }
            return courses;
        }

        private List<Student> LoadStudents(IFormFile file)
        {
            List<Exam> exams = new List<Exam>();
            string filePath = Path.GetTempFileName();

            using (var stream = File.Create(filePath))
            {
                file.CopyTo(stream);
            }
            List<Student> students = new List<Student>();
            foreach (var line in File.ReadLines(filePath).Skip(1)) // Skip header
            {
                string[] columns = line.Split(',');
                Student student = new Student
                {
                    StudentId = int.Parse(columns[0]),
                    StudentName = columns[1],
                    CourseIds = columns[2].Split(';').Select(int.Parse).ToList()
                };

                //assigned students to courses
                foreach (int course in student.CourseIds)
                    Courses.First(x => x.CourseId == course).Students.Add(student);
                students.Add(student);
            }

            return students;
        }

        private List<Room> LoadRooms(IFormFile file)
        {
            List<Exam> exams = new List<Exam>();
            string filePath = Path.GetTempFileName();

            using (var stream = File.Create(filePath))
            {
                file.CopyTo(stream);
            }
            var rooms = new List<Room>();
            foreach (var line in File.ReadLines(filePath).Skip(1)) // Skip header
            {
                string[] columns = line.Split(',');
                Room room = new Room
                {
                    RoomId = columns[0],
                    Capacity = int.Parse(columns[1])
                };
                rooms.Add(room);
            }
            return rooms;
        }

        private List<TimeSlot> LoadTimeSlots(IFormFile file)
        {
            var exams = new List<Exam>();
            var filePath = Path.GetTempFileName();

            using (FileStream stream = File.Create(filePath))
            {
                file.CopyTo(stream);
            }
            List<TimeSlot> timeSlots = new List<TimeSlot>();
            foreach (var line in File.ReadLines(filePath).Skip(1)) // Skip header
            {
                string[] columns = line.Split(',');
                TimeSlot timeSlot = new TimeSlot
                {
                    SlotId = int.Parse(columns[0]),
                    StartTime = DateTime.Parse(columns[1]),
                    Duration = double.Parse(columns[2])
                };
                timeSlots.Add(timeSlot);
            }
            return timeSlots;
        }

        public Schedule GenerateSchedule()
        {
            Schedule schedule = new Schedule();
            schedule.Exams = new List<Exam>();
            Random rand = new Random();
            int i = 0;
            foreach (Course course in Courses)
            {
                List<Student> remainingStudents = new(course.Students);
                while (remainingStudents.Any())
                {
                    Room room = Rooms[rand.Next(Rooms.Count)];
                    TimeSlot timeSlot = TimeSlots[rand.Next(TimeSlots.Count)];
                    List<Student> students = remainingStudents.Take(room.Capacity).ToList();
                    remainingStudents = remainingStudents.Except(students).ToList();

                    schedule.Exams.Add(new Exam()
                    {
                        ExamId = i++,
                        Course = course,
                        Room = room,
                        TimeSlot = timeSlot,
                        Students = students
                    });
                }
            }
            schedule.CalculateConflict();
            return schedule;
        }

        public Schedule Mutate(Schedule child)
        {
            double mutateRatio = 0.1;
            if (rand.NextDouble() < mutateRatio)
            {
                TimeSlot timeSlot = TimeSlots[rand.Next(TimeSlots.Count)];
                child.Exams[rand.Next(child.Exams.Count)].TimeSlot = timeSlot;
            }
            return child;
        }

        public Schedule Crossover(Schedule parent1, Schedule parent2)
        {
            Schedule child = new Schedule();
            child.Exams = new List<Exam>();
            foreach (Course course in Courses)
            {
                int val = rand.Next(2);
                if (val == 0)
                    child.Exams.AddRange(parent1.Exams.Where(x => x.Course == course));
                else
                    child.Exams.AddRange(parent2.Exams.Where(x => x.Course == course));
            }
            return child;
        }
    }
}
