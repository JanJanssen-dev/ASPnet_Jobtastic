using ASPnet_Jobtastic.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace ASPnet_Jobtastic.Controllers
{
    [Route("api/jobposting")]
    [ApiController]

    public class ApiJobPostingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiJobPostingController(ApplicationDbContext context)
        {
            
            _context = context;
        }
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var allJobPostings = _context.JobPostings.ToList();
            return Ok(allJobPostings);
        }
        [HttpGet("GetById")]
        public IActionResult GetById(int id)
        {

            var getJobPostingById = _context.JobPostings.SingleOrDefault(x => x.Id == id);

            if (getJobPostingById == null)
            {
                return NotFound();
            }
            return Ok(getJobPostingById);
        }
    }
}
