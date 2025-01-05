using SchedualTest.Models.Entities;
using System.Collections.Generic;

namespace SchedualTest.Models.Logic
{
    public class GAOperations
    {
        private List<Schedule> population = new List<Schedule>();
        private Random rand = new Random();
        private int _populationSize;
        private ExamScheduler _scheduler;
        public GAOperations(int populationSize, ExamScheduler scheduler)
        {
            _scheduler = scheduler;
            _populationSize = populationSize;
            for (int i = 0; i < populationSize; i++)
                population.Add(scheduler.GenerateSchedule());

            population = population.OrderByDescending(x => x.Fitness).ToList();
        }

        public GAResult RunGA(int maxGererations)
        {
            int generationNo = 1;
            while (population[0].Fitness < 1)
            {
                if (generationNo > maxGererations)
                    throw new Exception($"It takes more than {maxGererations} gererations. May has to increase time slots");

                List<Schedule> newPopulation = population.Take(population.Count / 2).ToList();
                //Chance wheel
                double[] pointRange = new double[newPopulation.Count + 1];
                pointRange[0] = newPopulation.Sum(x => x.Fitness);

                for (int i = 1; i < newPopulation.Count; i++)
                    pointRange[i] = pointRange[i - 1] - newPopulation[i].Fitness;

                List<Schedule> newChildren = new List<Schedule>();

                while (newChildren.Count < _populationSize - newPopulation.Count)
                {
                    //selection
                    double totalFitness = newPopulation.Sum(x => x.Fitness);
                    var val1 = rand.NextDouble();
                    var val2 = rand.NextDouble();
                    Schedule parent1 = null;
                    Schedule parent2 = null;
                    for (int i = 1; i < pointRange.Length; i++)
                    {
                        if (val1 >= pointRange[i] && parent1 == null)
                            parent1 = newPopulation[i - 1];

                        if (val2 >= pointRange[i] && parent2 == null)
                            parent2 = newPopulation[i - 1];

                        if (parent1 != null && parent2 != null)
                            break;
                    }

                    //crossover then mutate
                    Schedule child = _scheduler.Crossover(parent1, parent2);
                    child = _scheduler.Mutate(child);
                    child.CalculateConflict();
                    newChildren.Add(child);
                }
                //update population
                newPopulation.AddRange(newChildren);
                population = new(newPopulation.OrderByDescending(x => x.Fitness).ToList());
                generationNo++;
            }
            Schedule bestSchedule = population[0];
            //bestSchedule.Exams = bestSchedule.Exams.OrderBy(x => new { x.Course.CourseName, x.TimeSlot.StartTime }).ToList();
            return new GAResult()
            {
                RunTimes = generationNo,
                BestSchedule = bestSchedule,
                StudentsExmas = ExtractStudentsExams(bestSchedule.Exams)
            };
        }

        private Dictionary<Student, List<Exam>> ExtractStudentsExams(List<Exam> exams)
        {
            Dictionary<Student, List<Exam>> studentsExams = new Dictionary<Student, List<Exam>>();
            foreach (Exam exam in exams)
            {
                foreach (Student student in exam.Students)
                {
                    if (!studentsExams.ContainsKey(student))
                        studentsExams[student] = new List<Exam>();
                    studentsExams[student].Add(exam);
                }
            }
            return studentsExams;
        }
    }
}
