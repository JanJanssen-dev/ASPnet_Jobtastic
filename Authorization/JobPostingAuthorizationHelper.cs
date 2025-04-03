using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPnet_Jobtastic.Authorization
{
    public class JobPostingAuthorizationHelper
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<JobPostingAuthorizationHelper> _logger;

        public JobPostingAuthorizationHelper(
            IAuthorizationService authorizationService,
            ILogger<JobPostingAuthorizationHelper> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        /// <summary>
        /// Überprüft, ob der aktuelle Benutzer berechtigt ist, die angegebene Operation auf dem JobPosting auszuführen
        /// </summary>
        /// <param name="user">Der aktuelle Benutzer</param>
        /// <param name="controller">Der Controller für die Rückgabe des Ergebnisses</param>
        /// <param name="jobId">Die ID des JobPostings</param>
        /// <param name="operation">Die Operation (View, Edit, Delete, ManageSharing)</param>
        /// <param name="returnUrl">Die URL, zu der zurückgekehrt werden soll (optional)</param>
        /// <returns>Null, wenn die Autorisierung erfolgreich ist, andernfalls ein IActionResult</returns>
        public async Task<IActionResult> AuthorizeJobPostingAsync(
            System.Security.Claims.ClaimsPrincipal user,
            Controller controller,
            int jobId,
            string operation,
            string returnUrl = null)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                user,
                jobId,
                new JobPostingAuthorizationRequirement(operation));

            if (authorizationResult.Succeeded)
            {
                return null; // Autorisierung erfolgreich
            }

            _logger.LogWarning("Unbefugter Zugriff auf JobPosting: {JobId}, Operation: {Operation}",
                jobId, operation);

            if (user?.Identity?.IsAuthenticated != true)
            {
                // Benutzer ist nicht angemeldet
                return controller.RedirectToAction("Login", "Account", new { area = "Identity", returnUrl });
            }

            // Verwende TempData für die Fehlermeldung und leite direkt zurück
            controller.TempData["ErrorMessage"] = "Zugriff verweigert. Sie haben keine Berechtigung für diese Aktion.";
            return controller.RedirectToAction("Index", "JobPosting");
        }
    }
}