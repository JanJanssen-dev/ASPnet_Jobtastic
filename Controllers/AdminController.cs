using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ASPnet_Jobtastic.Data;

namespace ASPnet_Jobtastic.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && await _roleManager.RoleExistsAsync(role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Rolle {Role} für Benutzer {UserId} hinzugefügt", role, userId);
                        TempData["SuccessMessage"] = $"Rolle '{role}' wurde erfolgreich hinzugefügt.";
                    }
                    else
                    {
                        _logger.LogWarning("Fehler beim Hinzufügen der Rolle {Role} für Benutzer {UserId}", role, userId);
                        TempData["ErrorMessage"] = "Fehler beim Hinzufügen der Rolle.";
                    }
                }
                else
                {
                    _logger.LogWarning("Benutzer oder Rolle nicht gefunden: UserId={UserId}, Role={Role}", userId, role);
                    TempData["ErrorMessage"] = "Benutzer oder Rolle nicht gefunden.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Hinzufügen der Rolle {Role} für Benutzer {UserId}", role, userId);
                TempData["ErrorMessage"] = "Ein Fehler ist aufgetreten.";
            }

            // Zur Admin-Panel-Seite zurückleiten anstatt zur Index-Action
            return RedirectToPage("/Account/Manage/AdminPanel", new { area = "Identity" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Rolle {Role} für Benutzer {UserId} entfernt", role, userId);
                        TempData["SuccessMessage"] = $"Rolle '{role}' wurde erfolgreich entfernt.";
                    }
                    else
                    {
                        _logger.LogWarning("Fehler beim Entfernen der Rolle {Role} für Benutzer {UserId}", role, userId);
                        TempData["ErrorMessage"] = "Fehler beim Entfernen der Rolle.";
                    }
                }
                else
                {
                    _logger.LogWarning("Benutzer oder Rolle nicht gefunden: UserId={UserId}, Role={Role}", userId, role);
                    TempData["ErrorMessage"] = "Benutzer oder Rolle nicht gefunden.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Entfernen der Rolle {Role} für Benutzer {UserId}", role, userId);
                TempData["ErrorMessage"] = "Ein Fehler ist aufgetreten.";
            }

            // Zur Admin-Panel-Seite zurückleiten anstatt zur Index-Action
            return RedirectToPage("/Account/Manage/AdminPanel", new { area = "Identity" });
        }

        // Action für AdminPanel-Partialview
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PanelContent()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();

            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = userRoles.ToList()
                });
            }

            ViewBag.AllRoles = roles.Select(r => r.Name).ToList();
            return PartialView("_AdminPanelContent", model);
        }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}