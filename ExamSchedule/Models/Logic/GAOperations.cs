using ExamSchedule.Models.Entities;

namespace ExamSchedule.Models.Logic
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
            //order population descending deneding on fitness value
            population = population.OrderByDescending(x => x.Fitness).ToList();
        }

        public GAResult RunGA(int maxGenerations)
        {
            int generationCount = 1;
            //check if a child has fitness equal to 1 or not
            while (population[0].Fitness < 1)
            {
                //check if generation count grater than max generations
                if (generationCount > maxGenerations)
                    throw new Exception($"It takes more than {maxGenerations} generations. May has to increase time slots");

                //initiate a new generation (population)
                //start with a half count of ordered current population
                //to add generated children later 
                List<Schedule> newPopulation = population.Take(population.Count / 2).ToList();

                //container to add generated children
                //for adding them later to new population
                List<Schedule> newChildren = new List<Schedule>();

                // creating a chance wheel depending on fitness values for all children (schedule)
                double[] pointRange = new double[newPopulation.Count + 1];
                pointRange[0] = newPopulation.Sum(x => x.Fitness);
                for (int i = 1; i < newPopulation.Count; i++)
                    pointRange[i] = pointRange[i - 1] - newPopulation[i].Fitness;

                while (newChildren.Count < _populationSize - newPopulation.Count)
                {
                    //selection parents
                    Schedule parent1, parent2;
                    Selection(newPopulation, pointRange, out parent1, out parent2);

                    //generate a child by crossover parents
                    Schedule child = _scheduler.Crossover(parent1, parent2);

                    //exposing generated child to a mutation
                    child = _scheduler.Mutate(child);

                    //calculate generated child conflicts
                    child.CalculateConflict();

                    //add generated child (new individual) to generated children container
                    newChildren.Add(child);
                }

                //incorporating children into the new population
                newPopulation.AddRange(newChildren);

                //order new population descendingly
                //and replace current population with new one
                population = new(newPopulation.OrderByDescending(x => x.Fitness).ToList());

                //increase generation count
                generationCount++;
            }

            //best children which has the best fitness value equal to one
            //and should be the first individual of population because of ordering descendingly
            Schedule bestSchedule = population[0];
            return new GAResult()
            {
                GenerationCount = generationCount,
                BestSchedule = bestSchedule,
                //aggregates exams for each student
                StudentsExmas = ExtractStudentsExams(bestSchedule.Exams)
            };
        }

        private void Selection(List<Schedule> newPopulation, double[] pointRange, out Schedule parent1, out Schedule parent2)
        {
            var val1 = rand.NextDouble();
            var val2 = rand.NextDouble();
            parent1 = null;
            parent2 = null;
            for (int i = 1; i < pointRange.Length; i++)
            {
                if (parent1 == null && val1 >= pointRange[i])
                    parent1 = newPopulation[i - 1];

                if (parent2 == null && val2 >= pointRange[i])
                    parent2 = newPopulation[i - 1];

                if (parent1 != null && parent2 != null)
                    break;
            }
        }

        private Dictionary<Student, List<Exam>> ExtractStudentsExams(List<Exam> exams)
        {
            Dictionary<Student, List<Exam>> studentsExams = new Dictionary<Student, List<Exam>>();
            foreach (Exam exam in exams)
                foreach (Student student in exam.Students)
                {
                    if (!studentsExams.ContainsKey(student))
                        studentsExams[student] = new List<Exam>();
                    studentsExams[student].Add(exam);
                }

            return studentsExams;
        }
    }
}
