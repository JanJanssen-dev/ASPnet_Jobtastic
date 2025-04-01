using ASPnet_Jobtastic.Areas.Identity.Pages.Account;
using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ASPnet_Jobtastic.Controllers
{
    public class JobPostingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JobPostingController> _logger;

        public JobPostingController(ApplicationDbContext context, ILogger<JobPostingController> logger)
        {
            _context = context;
            _logger = logger;
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

            if (file != null && file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    jobPostingModel.CompanyImage = ms.ToArray();
                }
            }
            else
            {
                // Wenn kein Bild hochgeladen wurde, Standardbild aus wwwroot/images verwenden
                var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-logo.png");

                if (System.IO.File.Exists(defaultPath))
                {
                    jobPostingModel.CompanyImage = System.IO.File.ReadAllBytes(defaultPath);
                }
                else
                {
                    // Fallback, falls Standardbild fehlt
                    jobPostingModel.CompanyImage = Array.Empty<byte>();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.JobPostings.Add(jobPostingModel);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Speichern des JobPostings");
                    throw;
                }
                LogModelErrors();
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

            // Bildverarbeitung
            if (file != null && file.Length > 0)
            {
                // Neues Bild wurde hochgeladen
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    jobPostingModel.CompanyImage = ms.ToArray();
                }
            }
            else if (existingJob.CompanyImage != null && existingJob.CompanyImage.Length > 0)
            {
                // Altes Bild weiterverwenden
                jobPostingModel.CompanyImage = existingJob.CompanyImage;
            }
            else
            {
                // Standardbild aus wwwroot verwenden
                var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-logo.png");

                if (System.IO.File.Exists(defaultPath))
                {
                    jobPostingModel.CompanyImage = System.IO.File.ReadAllBytes(defaultPath);
                }
                else
                {
                    // Fallback falls das Standardbild fehlt
                    jobPostingModel.CompanyImage = Array.Empty<byte>();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobPostingModel);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_context.JobPostings.Any(e => e.Id == jobPostingModel.Id))
                        return NotFound();
                    else
                    {
                        _logger.LogError(ex, "Fehler beim Aktualisieren des JobPostings");
                        throw;
                    }
                }
                LogModelErrors();
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

        private void LogModelErrors()
        {
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    _logger.LogWarning("Fehler bei Feld {Field}: {Error}", entry.Key, error.ErrorMessage);
                }
            }
        }
    }
}

