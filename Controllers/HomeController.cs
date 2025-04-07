using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ASPnet_Jobtastic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Lade alle aktiven Jobinserate mit minimalen Daten
            var jobPostings = _context.JobPostings
                .Select(j => new JobPostingModel
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    CompanyName = j.CompanyName,
                    JobLocation = j.JobLocation,
                    Salary = j.Salary,
                    StartDate = j.StartDate,
                    CreationDate = j.CreationDate,
                    CompanyImage = j.CompanyImage
                })
                .OrderByDescending(j => j.CreationDate)
                .ToList();

            return View(jobPostings);
        }

        // Neuer Endpoint für Jobdetails
        [HttpGet]
        public IActionResult GetJobDetails(int id)
        {
            var job = _context.JobPostings
                .Select(j => new
                {
                    j.Id,
                    j.JobTitle,
                    j.JobDescription,
                    j.CompanyName,
                    j.JobLocation,
                    j.StartDate,
                    j.Salary,
                    j.CompanyImage,
                    j.ContactName,
                    j.ContactEmail,
                    j.CompanyWebsite
                })
                .FirstOrDefault(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return Json(job);
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