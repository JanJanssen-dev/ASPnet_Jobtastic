using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPnet_Jobtastic.Controllers
{
    [Authorize] // Stellt sicher, dass nur angemeldete Benutzer zugreifen können
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
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var jobPostingsFromDb = _context.JobPostings
                .Where(x => x.OwnerUsername == username)
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
        public IActionResult CreateJob(JobPostingModel jobPostingModel, IFormFile CompanyImage)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            jobPostingModel.OwnerUsername = username;
            // Setze Erstellungsdatum
            jobPostingModel.CreationDate = DateTime.Now;
            // Bei Erstellung ist der ändernde Benutzer der Ersteller
            jobPostingModel.ChangeDate = DateTime.Now;
            jobPostingModel.ChangeUserName = username;

            if (CompanyImage != null && CompanyImage.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        CompanyImage.CopyTo(ms);
                        jobPostingModel.CompanyImage = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Verarbeiten des Bildes");
                    ModelState.AddModelError("CompanyImage", "Fehler beim Verarbeiten des Bildes. Bitte versuchen Sie es erneut.");
                }
            }
            else
            {
                // Wenn kein Bild hochgeladen wurde, Standardbild aus wwwroot/images verwenden
                var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default-logo.png");

                if (System.IO.File.Exists(defaultPath))
                {
                    jobPostingModel.CompanyImage = System.IO.File.ReadAllBytes(defaultPath);
                    ModelState.Remove("CompanyImage");
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
                    _logger.LogInformation("Neues JobPosting erstellt: {JobId} von {Username}", jobPostingModel.Id, username);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Speichern des JobPostings");
                    ModelState.AddModelError("", "Fehler beim Speichern des JobPostings. Bitte versuchen Sie es später erneut.");
                }
            }

            LogModelErrors();
            return View("CreatedEditJobPosting", jobPostingModel);
        }

        // GET: Bestehendes JobPosting bearbeiten
        public IActionResult EditJob(int id)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var job = _context.JobPostings.Find(id);
            if (job == null)
                return NotFound();

            // Überprüfen, ob der Benutzer der Besitzer des JobPostings ist
            if (job.OwnerUsername != username)
            {
                _logger.LogWarning("Unbefugter Zugriff auf JobPosting: {JobId} von {Username}", id, username);
                return Forbid();
            }

            return View("CreatedEditJobPosting", job);
        }

        // POST: Bearbeitetes JobPosting speichern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditJob(int id, JobPostingModel jobPostingModel, IFormFile CompanyImage)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (id != jobPostingModel.Id)
                return BadRequest();

            var existingJob = _context.JobPostings.AsNoTracking().FirstOrDefault(j => j.Id == id);
            if (existingJob == null)
                return NotFound();

            // Überprüfen, ob der Benutzer der Besitzer des JobPostings ist
            if (existingJob.OwnerUsername != username)
            {
                _logger.LogWarning("Unbefugter Zugriff auf JobPosting: {JobId} von {Username}", id, username);
                return Forbid();
            }

            // Setze neue Änderungsinformationen
            jobPostingModel.ChangeDate = DateTime.Now;
            jobPostingModel.ChangeUserName = username;

            // Bewahre das ursprüngliche Erstellungsdatum
            jobPostingModel.CreationDate = existingJob.CreationDate;
            jobPostingModel.OwnerUsername = existingJob.OwnerUsername;

            // Bildverarbeitung
            if (CompanyImage != null && CompanyImage.Length > 0)
            {
                try
                {
                    // Neues Bild wurde hochgeladen
                    using (var ms = new MemoryStream())
                    {
                        CompanyImage.CopyTo(ms);
                        jobPostingModel.CompanyImage = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Verarbeiten des Bildes");
                    ModelState.AddModelError("CompanyImage", "Fehler beim Verarbeiten des Bildes. Bitte versuchen Sie es erneut.");
                }
            }
            else if (existingJob.CompanyImage != null && existingJob.CompanyImage.Length > 0)
            {
                // Altes Bild weiterverwenden
                jobPostingModel.CompanyImage = existingJob.CompanyImage;
                // Entferne die Validierungsfehlermeldung für CompanyImage, da das vorhandene Bild verwendet wird
                ModelState.Remove("CompanyImage");
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
                    _logger.LogInformation("JobPosting aktualisiert: {JobId} von {Username}", id, username);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_context.JobPostings.Any(e => e.Id == jobPostingModel.Id))
                        return NotFound();
                    else
                    {
                        _logger.LogError(ex, "Fehler beim Aktualisieren des JobPostings");
                        ModelState.AddModelError("", "Fehler beim Aktualisieren des JobPostings. Bitte versuchen Sie es später erneut.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unerwarteter Fehler beim Aktualisieren des JobPostings");
                    ModelState.AddModelError("", "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es später erneut.");
                }
            }

            LogModelErrors();
            return View("CreatedEditJobPosting", jobPostingModel);
        }

        // POST: JobPosting löschen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteJob(int id)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var job = _context.JobPostings.Find(id);
            if (job == null)
                return NotFound();

            // Überprüfen, ob der Benutzer der Besitzer des JobPostings ist
            if (job.OwnerUsername != username)
            {
                _logger.LogWarning("Unbefugter Löschversuch auf JobPosting: {JobId} von {Username}", id, username);
                return Forbid();
            }

            try
            {
                _context.JobPostings.Remove(job);
                _context.SaveChanges();
                _logger.LogInformation("JobPosting gelöscht: {JobId} von {Username}", id, username);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Löschen des JobPostings: {JobId}", id);
                TempData["ErrorMessage"] = "Fehler beim Löschen des Job-Inserats. Bitte versuchen Sie es später erneut.";
                return RedirectToAction(nameof(Index));
            }
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