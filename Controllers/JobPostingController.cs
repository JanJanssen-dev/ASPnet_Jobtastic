using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASPnet_Jobtastic.Controllers
{
    public class JobPostingController : Controller
    {
        //DB verbindung herstellen im Controller
        private readonly ApplicationDbContext _context;
        public JobPostingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var jobPostingsFromDb = _context.JobPostings.Where(x => x.OwnerUsername == User.Identity.Name).ToList();
            return View(jobPostingsFromDb);
        }

        public IActionResult CreatedEditJobPosting(int id)
        {
            return View();
        }

        public IActionResult CreateEditJob(JobPostingModel jobPostingModel, IFormFile file)
        {

            //Username Abfangen
            jobPostingModel.OwnerUsername = User.Identity.Name;

            if (file != null)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var bytes = ms.ToArray();
                    jobPostingModel.CompanyImage = bytes;
                }
            }
            _context.JobPostings.Add(jobPostingModel);
            _context.SaveChanges();   


            return RedirectToAction("Index");
        }
    }
}