using ASPnet_Jobtastic.Authorization;
using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using ASPnet_Jobtastic.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPnet_Jobtastic.Controllers
{
    [Authorize] // Stellt sicher, dass nur angemeldete Benutzer zugreifen können
    public class JobPostingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JobPostingController> _logger;
        private readonly JobPostingAuthorizationHelper _authHelper;
        private readonly UserManager<IdentityUser> _userManager;

        public JobPostingController(
            ApplicationDbContext context,
            ILogger<JobPostingController> logger,
            JobPostingAuthorizationHelper authHelper,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _authHelper = authHelper;
            _userManager = userManager;
        }

        // Übersicht aller JobPostings mit Filteroption für Admins
        public async Task<IActionResult> Index(bool showAllJobs = false)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Prüfe, ob Benutzer Admin ist
            var user = await _userManager.FindByNameAsync(username);
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Administrator");

            // Liste der Jobs, die der Nutzer sehen soll
            List<JobPostingModel> allJobPostings = new List<JobPostingModel>();

            // Eigene Postings
            var ownedJobPostings = await _context.JobPostings
                .Where(x => x.OwnerUsername == username)
                .ToListAsync();

            // Setze Eigenschaften für eigene Jobs
            foreach (var job in ownedJobPostings)
            {
                job.IsOwner = true;
                job.IsAdmin = isAdmin;
                job.CanEdit = true;
                job.CanDelete = true;
                allJobPostings.Add(job);
            }

            // Freigaben anderer Benutzer
            var sharedJobPostingIds = await _context.JobSharings
                .Where(x => x.SharedUsername == username)
                .Select(x => x.JobPostingId)
                .ToListAsync();

            var sharedJobPostings = await _context.JobPostings
                .Where(j => sharedJobPostingIds.Contains(j.Id))
                .ToListAsync();

            foreach (var job in sharedJobPostings)
            {
                job.IsOwner = false;
                job.IsAdmin = isAdmin;
                job.CanEdit = true;
                job.CanDelete = true;
                allJobPostings.Add(job);
            }

            // Wenn Admin und showAllJobs ist true, füge alle anderen Jobs hinzu
            if (isAdmin && showAllJobs)
            {
                var existingJobIds = allJobPostings.Select(j => j.Id).ToList();

                var adminJobs = await _context.JobPostings
                    .Where(j => !existingJobIds.Contains(j.Id))
                    .ToListAsync();

                foreach (var job in adminJobs)
                {
                    job.IsOwner = false;
                    job.IsAdmin = true;
                    job.CanEdit = true;
                    job.CanDelete = true;
                    job.IsAdminView = true;  // Markiere Jobs, die nur über Admin-Ansicht sichtbar sind
                    allJobPostings.Add(job);
                }
            }

            // ViewBag-Eigenschaften für View-Logik
            ViewBag.IsAdmin = isAdmin;
            ViewBag.ShowAllJobs = showAllJobs;

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
        public async Task<IActionResult> CreateJob(JobPostingModel jobPostingModel, IFormFile CompanyImage)
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
                    await _context.SaveChangesAsync();

                    SweetAlertMessage("Job gespeichert", "Der Job wurde erfolgreich gespeichert.", "success");

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
        public async Task<IActionResult> EditJob(int id)
        {
            // Autorisierung prüfen
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, id, JobPostingOperations.Edit);
            if (authResult != null)
            {
                return authResult;
            }

            var job = await _context.JobPostings.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            return View("CreatedEditJobPosting", job);
        }

        // POST: Bearbeitetes JobPosting speichern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(int id, JobPostingModel jobPostingModel, IFormFile CompanyImage)
        {
            // Autorisierung prüfen
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, id, JobPostingOperations.Edit);
            if (authResult != null)
            {
                return authResult;
            }

            if (id != jobPostingModel.Id)
            {
                return BadRequest();
            }

            // Den Kontext für das existierende JobPosting löschen, um Tracking-Konflikte zu vermeiden
            _context.ChangeTracker.Clear();

            var existingJob = await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == id);
            if (existingJob == null)
            {
                return NotFound();
            }

            // Bewahre Originaldaten
            jobPostingModel.OwnerUsername = existingJob.OwnerUsername;
            jobPostingModel.CreationDate = existingJob.CreationDate;

            // Setze Änderungsinformationen
            jobPostingModel.ChangeDate = DateTime.Now;
            jobPostingModel.ChangeUserName = User.Identity?.Name;

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
                    // Wir fügen das Entity als "Modified" hinzu, nachdem wir den Tracker geleert haben
                    _context.JobPostings.Update(jobPostingModel);
                    await _context.SaveChangesAsync();

                    SweetAlertMessage("Job gespeichert", "Der Job wurde erfolgreich gespeichert.", "success");

                    _logger.LogInformation("JobPosting aktualisiert: {JobId} von {Username}", id, User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_context.JobPostings.Any(e => e.Id == jobPostingModel.Id))
                    {
                        return NotFound();
                    }
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
        public async Task<IActionResult> DeleteJob(int id)
        {
            // Autorisierung prüfen
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, id, JobPostingOperations.Delete);
            if (authResult != null)
            {
                return authResult;
            }

            var job = await _context.JobPostings.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            try
            {
                _context.JobPostings.Remove(job);
                await _context.SaveChangesAsync();

                SweetAlertMessage("Job gelöscht", "Der Job wurde erfolgreich gelöscht.", "success");

                _logger.LogInformation("JobPosting gelöscht: {JobId} von {Username}", id, User.Identity?.Name);
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
        public async Task<IActionResult> ManageSharing(int id)
        {
            // Autorisierung prüfen - nur der Eigentümer darf Freigaben verwalten
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, id, JobPostingOperations.ManageSharing);
            if (authResult != null)
            {
                return authResult;
            }

            var job = await _context.JobPostings.FindAsync(id); 
            if (job == null)
            {
                return NotFound();
            }

            // Lade alle bestehenden Freigaben
            var sharings = await _context.JobSharings
                .Where(js => js.JobPostingId == id)
                .ToListAsync();

            var viewModel = new JobSharingViewModel
            {
                JobPosting = job,
                ExistingShares = sharings ?? new List<JobSharingModel>(), //zusätzliche NULL prüfung
                NewShare = new JobSharingModel { JobPostingId = id }
            };

            return View(viewModel);
        }

        // POST: Neue Freigabe hinzufügen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSharing(JobSharingViewModel model)
        {
            var jobId = model.NewShare.JobPostingId;

            // Autorisierung prüfen - nur der Eigentümer darf Freigaben hinzufügen
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, jobId, JobPostingOperations.ManageSharing);
            if (authResult != null)
            {
                return authResult;
            }

            // Prüfen, ob die Freigabe bereits existiert
            var existingShare = await _context.JobSharings
                .FirstOrDefaultAsync(js => js.JobPostingId == jobId &&
                                     js.SharedUsername == model.NewShare.SharedUsername);

            if (existingShare != null)
            {
                ModelState.AddModelError("NewShare.SharedUsername", "Dieser Benutzer hat bereits Zugriff auf dieses Inserat.");
                return RedirectToAction(nameof(ManageSharing), new { id = jobId });
            }

            // Neue Freigabe erstellen
            var newSharing = new JobSharingModel
            {
                JobPostingId = jobId,
                SharedUsername = model.NewShare.SharedUsername,
                SharedDate = DateTime.Now,
                SharedByUsername = User.Identity?.Name,
                CanEdit = model.NewShare.CanEdit,
                CanDelete = model.NewShare.CanDelete
            };

            _context.JobSharings.Add(newSharing);
            await _context.SaveChangesAsync();

            SweetAlertMessage("Freigabe gespeichert", "Die Freigabe wurde erfolgreich gespeichert.", "success");

            return RedirectToAction(nameof(ManageSharing), new { id = jobId });
        }

        // POST: Bearbeiten einer bestehenden Freigabe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSharing(int Id, int JobPostingId, bool CanEdit, bool CanDelete)
        {
            var sharing = await _context.JobSharings.FindAsync(Id);
            if (sharing == null)
            {
                return NotFound();
            }

            // Autorisierung prüfen - nur der Eigentümer darf Freigaben verwalten
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, JobPostingId, JobPostingOperations.ManageSharing);
            if (authResult != null)
            {
                return authResult;
            }

            // Berechtigungen aktualisieren (null bedeutet, dass die Checkbox nicht angekreuzt war)
            sharing.CanEdit = CanEdit;
            sharing.CanDelete = CanDelete;

            await _context.SaveChangesAsync();

            SweetAlertMessage("Freigabe gespeichert", "Die Freigabe wurde erfolgreich gespeichert.", "success");

            return RedirectToAction(nameof(ManageSharing), new { id = JobPostingId });
        }

        // POST: Freigabe löschen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSharing(int id)
        {
            var sharing = await _context.JobSharings
                .Include(js => js.JobPosting)
                .FirstOrDefaultAsync(js => js.Id == id);

            if (sharing == null)
            {
                return NotFound();
            }

            int jobId = sharing.JobPostingId;

            // Autorisierung prüfen - nur der Eigentümer darf Freigaben löschen
            var authResult = await _authHelper.AuthorizeJobPostingAsync(User, this, jobId, JobPostingOperations.ManageSharing);
            if (authResult != null)
            {
                return authResult;
            }

            _context.JobSharings.Remove(sharing);
            await _context.SaveChangesAsync();

            SweetAlertMessage("Freigabe entfernt", "Die Freigabe wurde erfolgreich entfernt.", "success");

            return RedirectToAction(nameof(ManageSharing), new { id = jobId });
        }

        private void SweetAlertMessage(string title, string text, string icon)
        {
            // Erfolgsmeldung setzen
            TempData["SweetAlert"] = new Dictionary<string, string> {
                { "title", title },
                { "text", text },
                { "icon", icon }
            };
        }
    }
}