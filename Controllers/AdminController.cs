using Microsoft.AspNetCore.Mvc;

namespace ASPnet_Jobtastic.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
