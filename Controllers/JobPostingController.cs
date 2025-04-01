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

        public IActionResult CreateJob(JobPostingModel jobPostingModel, IFormFile file)
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

        // GET: JobPosting/Edit/{ID}
        public IActionResult EditJob(int id) 
        {
            var job = _context.JobPostings.Find(id);
            if (job == null) return View("Error");
            return View("CreatedEditJobPosting",job);
        }

        // POST: JobPostings/Edit/{ID}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditJob(int id, JobPostingModel jobPostingModel)
        {
            if (id != jobPostingModel.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(jobPostingModel);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(jobPostingModel);
        }

        // POST: JobPosting/Delete/{ID}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteJob(int id)
        {
            var job = _context.JobPostings.Find(id);
            if (job == null) return NotFound();

            _context.JobPostings.Remove(job);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}