using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("user/getall")]
        public async Task<IActionResult> UserGetAll()
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            var logs = await _context.Logs
                .Include(l => l.Classroom)
                .Where(l => l.User.NeptunId == neptunId)
                .ToListAsync();

            return Ok(new { message = "Authorized", logs = logs });
        }

        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllLogs()
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });
            if (user.AdminLevel == AdminLevels.Student)
                return Unauthorized(new { message = "NotAuthorized" });

            var logs = await _context.Logs
                .Include(l => l.User)
                .Include(l => l.Classroom)
                .ToListAsync();

            return Ok(new { message = "Authorized", logs = logs });
        }
    }
}
