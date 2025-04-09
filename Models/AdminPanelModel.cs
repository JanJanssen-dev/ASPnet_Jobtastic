using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPnet_Jobtastic.Models
{
    [Authorize(Roles = "Administrator")]
    public class AdminPanelModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}