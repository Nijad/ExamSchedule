using ExamSchedule.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchedualTest.Models;
using SchedualTest.Models.Entities;
using SchedualTest.Models.Logic;
using System.Diagnostics;
using System.Text;

namespace SchedualTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Schedule item1;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("upload")]
        [HttpPost]
        public IActionResult Upload()
        {
            GAResult gaResult = new GAResult();
            IFormFileCollection files = HttpContext.Request.Form.Files;
            if (files == null || files.Count < 4)
            {
                gaResult.HasError = true;
                gaResult.ErrorMessage = "You have to upload all files.";
                return PartialView("ResultPV", gaResult);
            }

            //some validations need here

            IFormFile courseFile = files[0];
            IFormFile studentsFile = files[1];
            IFormFile roomsFile = files[2];
            IFormFile timeSlotsFile = files[3];
            ExamScheduler scheduler = null;
            try
            {
                scheduler = new ExamScheduler(courseFile, studentsFile, roomsFile, timeSlotsFile);
            }
            catch (Exception ex)
            {
                gaResult.HasError = true;
                gaResult.ErrorMessage = "Some files have wrong data.";
                return PartialView("ResultPV", gaResult);
            }

            int populationSize = 10;
            GAOperations ga = new GAOperations(populationSize, scheduler);


            try
            {
                gaResult = ga.RunGA(100000);
            }
            catch (Exception ex)
            {
                gaResult.HasError = true;
                gaResult.ErrorMessage = ex.Message;
                return PartialView("ResultPV", gaResult);
            }
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("Stuedent,Course,Room,Exam,Start Time,Duration");
            //foreach (Exam exam in gaResult.BestSchedule.Exams)
            //    foreach (Student student in exam.Students)
            //        sb.AppendLine($"{student.StudentId},{exam.Course.CourseName}," +
            //            $"{exam.Room.RoomId},{exam.ExamId},{exam.TimeSlot.StartTime}," +
            //            $"{exam.TimeSlot.Duration}");

            return PartialView("ResultPV", gaResult);
            //return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "Best Schedule.csv");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}