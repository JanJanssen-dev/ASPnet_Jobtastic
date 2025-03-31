using Microsoft.AspNetCore.Mvc;

namespace ASPnet_Jobtastic.Controllers
{
    public class JobPostingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreatedEditJobPosting(int id)
        {
            return View();
        }

        public IActionResult CreateEditJob()
        {
            return View(); //das falsch
        }
    }
}
