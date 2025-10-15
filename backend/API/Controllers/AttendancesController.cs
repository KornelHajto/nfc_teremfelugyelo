using API.Data;
using API.DTOs;
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
    public class AttendancesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AttendancesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPut("edit")]
        public async Task<IActionResult> EditAttendance([FromBody] EditAttendanceDTO attendance)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? admin = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (admin == null)
                return Unauthorized(new { message = "NoTeacherFound" });
            if (admin.AdminLevel == AdminLevels.Student)
                return Unauthorized(new { message = "NotAuthorized" });
            Attendance? toEdit = await _context.Attendances.FirstOrDefaultAsync(c => c.Id == attendance.Id);
            if (toEdit == null)
                return NotFound(new { message = "AttendanceNotFound" });
            toEdit.AttendanceType = attendance.AttendanceType;
            toEdit.Comment = attendance.Comment;
            await _context.SaveChangesAsync();
            return Ok(new {message = "AttendanceUpdated"});
        }

        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllAttendances()
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
            List<Attendance> attendances = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Subject)
                .ToListAsync();

            return Ok(new { message = "Authorized", attendances = attendances });

        }

        [Authorize]
        [HttpGet("teacher/getall")]
        public async Task<IActionResult> GetAllAttendancesOfTeacher()
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? teacher = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (teacher == null)
                return Unauthorized(new { message = "NoUserFound" });
            if (teacher.AdminLevel == AdminLevels.Student)
                return Unauthorized(new { message = "NotAuthorized" });

            var subjectIds = await _context.Subjects
                .Where(s => s.Teachers.Any(t => t.NeptunId == neptunId))
                .Select(s => s.Id)
                .ToListAsync();

            var attendances = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Subject)
                .Where(a => subjectIds.Contains(a.Subject.Id))
                .ToListAsync();

            return Ok(new { message = "Authorized", attendances });
        }

    }
}
