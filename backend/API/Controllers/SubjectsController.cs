using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDTO subject)
        {
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            if (user.AdminLevel != AdminLevels.Admin)
                return Unauthorized(new { message = "NoPermission" });

            bool exists = await _context.Subjects.AnyAsync(c => c.Id == subject.Id);
            if (exists)
            {
                return Conflict(new { message = "SubjectIdTaken" });
            }
            Subject newSubject = new()
            {
                Id = subject.Id,
                Name = subject.Name
            };
            await _context.Subjects.AddAsync(newSubject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "SubjectCreated" });
        }
    }
}
