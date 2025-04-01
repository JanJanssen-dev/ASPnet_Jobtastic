using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPnet_Jobtastic.Controllers
{
    public class JobPostingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobPostingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Übersicht aller JobPostings des eingeloggten Users
        public IActionResult Index()
        {
            var jobPostingsFromDb = _context.JobPostings
                .Where(x => x.OwnerUsername == User.Identity.Name)
                .ToList();
            return View(jobPostingsFromDb);
        }

        // GET: Neues JobPosting anlegen
        public IActionResult CreateJob()
        {
            return View("CreatedEditJobPosting", new JobPostingModel());
        }

        // POST: Neues JobPosting speichern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateJob(JobPostingModel jobPostingModel, IFormFile file)
        {
            jobPostingModel.OwnerUsername = User.Identity.Name;

            if (file != null)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    jobPostingModel.CompanyImage = ms.ToArray();
                }
            }

            if (ModelState.IsValid)
            {
                _context.JobPostings.Add(jobPostingModel);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View("CreatedEditJobPosting", jobPostingModel);
        }

        // GET: Bestehendes JobPosting bearbeiten
        public IActionResult EditJob(int id)
        {
            var job = _context.JobPostings.Find(id);
            if (job == null)
                return NotFound();

            return View("CreatedEditJobPosting", job);
        }

        // POST: Bearbeitetes JobPosting speichern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditJob(int id, JobPostingModel jobPostingModel, IFormFile file)
        {
            if (id != jobPostingModel.Id)
                return BadRequest();

            var existingJob = _context.JobPostings.AsNoTracking().FirstOrDefault(j => j.Id == id);
            if (existingJob == null)
                return NotFound();

            jobPostingModel.OwnerUsername = existingJob.OwnerUsername;

            if (file != null)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    jobPostingModel.CompanyImage = ms.ToArray();
                }
            }
            else
            {
                jobPostingModel.CompanyImage = existingJob.CompanyImage;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobPostingModel);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.JobPostings.Any(e => e.Id == jobPostingModel.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View("CreatedEditJobPosting", jobPostingModel);
        }

        // POST: JobPosting löschen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteJob(int id)
        {
            var job = _context.JobPostings.Find(id);
            if (job == null)
                return NotFound();

            _context.JobPostings.Remove(job);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
