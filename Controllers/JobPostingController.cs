using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using ASPnet_Jobtastic.Models.ViewModels;
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

            // Eigene Postings
            var ownedJobPostings = _context.JobPostings
                .Where(x => x.OwnerUsername == username)
                .ToList();

            // Freigaben anderer Benutzer
            var sharedJobPostingIds = _context.JobSharings
                .Where(x => x.SharedUsername == username)
                .Select(x => x.JobPostingId)
                .ToList();

            var sharedJobPostings = _context.JobPostings
                .Where(x => sharedJobPostingIds.Contains(x.Id))
                .ToList();

            // Beide Listen zusammenführen
            var allJobPostings = ownedJobPostings
                .Concat(sharedJobPostings)
                .OrderBy(x => x.CreationDate)
                .ToList();

            return View(allJobPostings);
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

            // Überprüfen, ob der Benutzer den Job bearbeiten darf
            if (!HasAccessToJobPosting(id, username, requireEditPermission: true))
            {
                _logger.LogWarning("Unbefugter Zugriff auf JobPosting: {JobId} von {Username}", id, username);
                return Content("<script>alert('Zugriff verweigert. Sie haben keine Berechtigung.'); history.back();</script>", "text/html");
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
            
            // Überprüfen, ob der Benutzer den Job bearbeiten darf
            if (!HasAccessToJobPosting(id, username, requireEditPermission: true))
            {
                _logger.LogWarning("Unbefugter Zugriff auf JobPosting: {JobId} von {Username}", id, username);
                return Content("<script>alert('Zugriff verweigert. Sie haben keine Berechtigung.'); history.back();</script>", "text/html");
            }

            // Bewahre Originaldaten
            jobPostingModel.OwnerUsername = existingJob.OwnerUsername;
            jobPostingModel.CreationDate = existingJob.CreationDate;

            // Setze Änderungsinformationen
            jobPostingModel.ChangeDate = DateTime.Now;
            jobPostingModel.ChangeUserName = username;

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

            // Überprüfen, ob der Benutzer den Job löschen darf //auslagern
            if (!HasAccessToJobPosting(id, username, requireDeletePermission: true))
            {
                _logger.LogWarning("Unbefugter Löschversuch auf JobPosting: {JobId} von {Username}", id, username);
                return Content("<script>alert('Zugriff verweigert. Sie haben keine Berechtigung.'); history.back();</script>", "text/html");
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
        // GET: Freigaben für ein JobPosting anzeigen
        public IActionResult ManageSharing(int id)
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
                _logger.LogWarning("Unbefugter Zugriff auf JobPosting-Freigaben: {JobId} von {Username}", id, username);
                return Content("<script>alert('Zugriff verweigert. Sie haben keine Berechtigung.'); history.back();</script>", "text/html");
            }

            // Lade alle bestehenden Freigaben
            var sharings = _context.JobSharings
                .Where(js => js.JobPostingId == id)
                .ToList();

            var viewModel = new JobSharingViewModel
            {
                JobPosting = job,
                ExistingShares = sharings,
                NewShare = new JobSharingModel { JobPostingId = id }
            };

            return View(viewModel);
        }

        // POST: Neue Freigabe hinzufügen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSharing(JobSharingViewModel model)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var job = _context.JobPostings.Find(model.NewShare.JobPostingId);
            if (job == null)
                return NotFound();

            // Überprüfen, ob der Benutzer der Besitzer des JobPostings ist
            if (job.OwnerUsername != username)
            {
                _logger.LogWarning("Unbefugter Zugriff auf JobPosting-Freigaben: {JobId} von {Username}", model.NewShare.JobPostingId, username);
                return Content("<script>alert('Zugriff verweigert. Sie haben keine Berechtigung.'); history.back();</script>", "text/html");
            }

            // Prüfen, ob der Benutzer existiert (optional)
            // ...

            // Prüfen, ob die Freigabe bereits existiert
            var existingShare = _context.JobSharings
                .FirstOrDefault(js => js.JobPostingId == model.NewShare.JobPostingId &&
                                      js.SharedUsername == model.NewShare.SharedUsername);

            if (existingShare != null)
            {
                ModelState.AddModelError("NewShare.SharedUsername", "Dieser Benutzer hat bereits Zugriff auf dieses Inserat.");
                return RedirectToAction(nameof(ManageSharing), new { id = model.NewShare.JobPostingId });
            }

            // Neue Freigabe erstellen
            var newSharing = new JobSharingModel
            {
                JobPostingId = model.NewShare.JobPostingId,
                SharedUsername = model.NewShare.SharedUsername,
                SharedDate = DateTime.Now,
                SharedByUsername = username,
                CanEdit = model.NewShare.CanEdit,
                CanDelete = model.NewShare.CanDelete
            };

            _context.JobSharings.Add(newSharing);
            _context.SaveChanges();

            return RedirectToAction(nameof(ManageSharing), new { id = model.NewShare.JobPostingId });
        }

        // POST: Freigabe löschen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSharing(int id)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var sharing = _context.JobSharings
                .Include(js => js.JobPosting)
                .FirstOrDefault(js => js.Id == id);

            if (sharing == null)
                return NotFound();

            // Überprüfen, ob der Benutzer der Besitzer des JobPostings ist
            if (sharing.JobPosting.OwnerUsername != username)
            {
                _logger.LogWarning("Unbefugter Zugriff beim Löschen einer Freigabe: {SharingId} von {Username}", id, username);
                return Content("<script>alert('Zugriff verweigert. Sie haben keine Berechtigung.'); history.back();</script>", "text/html");
            }

            _context.JobSharings.Remove(sharing);
            _context.SaveChanges();

            return RedirectToAction(nameof(ManageSharing), new { id = sharing.JobPostingId });
        }

        // Methode aktualisieren: Zugriff prüfen für Edit/Delete mit Freigaben
        private bool HasAccessToJobPosting(int jobId, string username, bool requireEditPermission = false, bool requireDeletePermission = false)
        {
            var job = _context.JobPostings.Find(jobId);
            if (job == null)
                return false;

            // Eigentümer hat immer alle Rechte
            if (job.OwnerUsername == username)
                return true;

            // Prüfe auf Freigabe
            var sharing = _context.JobSharings
                .FirstOrDefault(js => js.JobPostingId == jobId && js.SharedUsername == username);

            if (sharing == null)
                return false;

            // Prüfe spezifische Rechte wenn gefordert
            if (requireEditPermission && !sharing.CanEdit)
                return false;

            if (requireDeletePermission && !sharing.CanDelete)
                return false;

            return true;
        }
    }
}