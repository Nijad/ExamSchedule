using ExamSchedule.Models;
using Microsoft.AspNetCore.Mvc;
using ExamSchedule.Models.Logic;
using System.Diagnostics;

namespace ExamSchedule.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

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

            int populationSize = int.Parse(HttpContext.Request.Form["populationSize"]);
            int MaxGenerations = int.Parse(HttpContext.Request.Form["MaxGenerations"]);
            bool CalculateTimeSlot = bool.Parse(HttpContext.Request.Form["CalculateTimeSlot"]);
            int mustFileCount = 4;
            if (CalculateTimeSlot)
                mustFileCount = 3;

            IFormFileCollection files = HttpContext.Request.Form.Files;
            if (files == null || files.Count < mustFileCount)
            {
                gaResult.HasError = true;
                gaResult.ErrorMessage = "You have to upload all files.";
                return PartialView("ResultPV", gaResult);
            }
            //some validations need here

            IFormFile courseFile = files[0];
            IFormFile studentsFile = files[1];
            IFormFile roomsFile = files[2];

            ExamScheduler scheduler = null;
            if (!CalculateTimeSlot)
            {
                IFormFile timeSlotsFile = files[3];
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
            }
            else
            {
                if (string.IsNullOrEmpty(HttpContext.Request.Form["startDate"].ToString()) ||
                    string.IsNullOrEmpty(HttpContext.Request.Form["lastDate"].ToString()))
                {
                    gaResult.HasError = true;
                    gaResult.ErrorMessage = "Start and Last exam date must have a value";
                    return PartialView("ResultPV", gaResult);
                }

                double examDuration = double.Parse(HttpContext.Request.Form["examDuration"]);
                double gap = double.Parse(HttpContext.Request.Form["gap"]);
                DateTime startDate = DateTime.Parse(HttpContext.Request.Form["startDate"]);
                DateTime lastDate = DateTime.Parse(HttpContext.Request.Form["lastDate"]);
                if (startDate > lastDate)
                {
                    DateTime dateTime = startDate;
                    startDate = lastDate;
                    lastDate = dateTime;
                }

                int examPerDay = int.Parse(HttpContext.Request.Form["examPerDay"]);

                string[] days = HttpContext.Request.Form["holidays"]
                    .ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                int[] holidays = new int[days.Length];
                for (int i = 0; i < days.Length; i++)
                    holidays[i] = int.Parse(days[i]);

                try
                {
                    scheduler = new ExamScheduler(courseFile,
                        studentsFile, roomsFile, examDuration, gap,
                        startDate, lastDate, examPerDay, holidays);
                }
                catch (Exception ex)
                {
                    gaResult.HasError = true;
                    gaResult.ErrorMessage = "Some files have wrong data.";
                    return PartialView("ResultPV", gaResult);
                }
            }

            GAOperations ga = new GAOperations(populationSize, scheduler);

            try
            {
                gaResult = ga.RunGA(MaxGenerations);
            }
            catch (Exception ex)
            {
                gaResult.HasError = true;
                gaResult.ErrorMessage = ex.Message;
                return PartialView("ResultPV", gaResult);
            }

            return PartialView("ResultPV", gaResult);
        }

        public IActionResult About()
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