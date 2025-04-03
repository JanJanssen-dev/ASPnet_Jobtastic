// Diese Klasse implementiert die Logik für die Autorisierungsprüfung

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using Microsoft.EntityFrameworkCore;

namespace ASPnet_Jobtastic.Authorization
{
    public class JobPostingAuthorizationHandler :
        AuthorizationHandler<JobPostingAuthorizationRequirement, int>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<JobPostingAuthorizationHandler> _logger;

        public JobPostingAuthorizationHandler(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<JobPostingAuthorizationHandler> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    JobPostingAuthorizationRequirement requirement,
    int jobId)
        {
            // Benutzer abrufen
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return;
            }

            var username = user.UserName;

            // Überprüfen, ob der Benutzer Admin ist (für zukünftige Rollenerweiterung)
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                context.Succeed(requirement);
                return;
            }

            // JobPosting abrufen
            var jobPosting = await _context.JobPostings.FindAsync(jobId);
            if (jobPosting == null)
            {
                _logger.LogWarning("JobPosting mit ID {JobId} wurde nicht gefunden", jobId);
                return; // Job nicht gefunden, Zugriff verweigert
            }

            // Wenn der Benutzer der Eigentümer ist, hat er alle Rechte
            if (jobPosting.OwnerUsername == username)
            {
                context.Succeed(requirement);
                return;
            }

            // ManageSharing ist eine spezielle Operation, die nur dem Eigentümer erlaubt ist
            if (requirement.Operation == JobPostingOperations.ManageSharing)
            {
                // Da wir bereits überprüft haben, dass der aktuelle Benutzer nicht der Eigentümer ist,
                // verweigern wir den Zugriff für ManageSharing
                return; // Zugriff verweigert
            }

            // Für View-Operation, prüfen wir nur, ob eine Freigabe existiert
            if (requirement.Operation == JobPostingOperations.View)
            {
                // Bei View prüfen wir nur, ob eine Freigabe existiert
                var hasSharing = await _context.JobSharings
                    .AnyAsync(js => js.JobPostingId == jobId && js.SharedUsername == username);

                if (hasSharing)
                {
                    context.Succeed(requirement);
                }
                return;
            }

            // Für Edit, Delete müssen spezifische Berechtigungen geprüft werden
            var sharing = await _context.JobSharings
                .FirstOrDefaultAsync(js => js.JobPostingId == jobId && js.SharedUsername == username);

            if (sharing == null)
            {
                return; // Keine Freigabe gefunden, Zugriff verweigert
            }

            // Prüfe die spezifische Operation
            switch (requirement.Operation)
            {
                case var op when op == JobPostingOperations.Edit:
                    if (sharing.CanEdit)
                    {
                        context.Succeed(requirement);
                    }
                    break;

                case var op when op == JobPostingOperations.Delete:
                    if (sharing.CanDelete)
                    {
                        context.Succeed(requirement);
                    }
                    break;
            }
        }
    }
}