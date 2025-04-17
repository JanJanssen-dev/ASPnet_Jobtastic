using ASPnet_Jobtastic.Data;
using ASPnet_Jobtastic.Models;
using ASPnet_Jobtastic.Filters;
using Microsoft.AspNetCore.Authorization;
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
        [ApiKeyAuthorization]
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
        [ApiKeyAuthorization]
        [HttpPost("Create")]
        public IActionResult Create(JobPostingModel jobPostingModel)
        {
            if (jobPostingModel.Id != 0)
            {
                return BadRequest("Error: Darf keine Id enthalten!");
            }
            _context.JobPostings.Add(jobPostingModel);
            _context.SaveChanges();
            return Ok();
        }
        [ApiKeyAuthorization]
        [HttpDelete("Delete")]
        public IActionResult Delete(int id)
        {
            var getJobpostingById = _context.JobPostings.Find(id);

            if (getJobpostingById == null)
            {
                return NotFound();
            }
            _context.JobPostings.Remove(getJobpostingById);
            _context.SaveChanges();

            return Ok("Objekt wurde gelöscht");
        }
        [ApiKeyAuthorization]
        [HttpPut("Update")]
        public IActionResult Update(JobPostingModel jobPostingModel)
        {
            if (jobPostingModel.Id == 0)    
            {
                return BadRequest("Objekt hat keine ID");
            }
            _context.JobPostings.Update(jobPostingModel);
            _context.SaveChanges();
            return Ok("Objekt erfolgreich GeUpdatet");
        }
    }
}
