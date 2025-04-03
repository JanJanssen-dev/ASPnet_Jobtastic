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
            // Lade alle aktiven Jobinserate
            var jobPostings = _context.JobPostings
                //.Where(j => j.IsActive) // Optional: Falls ein IsActive-Flag existiert
                .OrderByDescending(j => j.CreationDate)
                .ToList();

            return View(jobPostings);
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