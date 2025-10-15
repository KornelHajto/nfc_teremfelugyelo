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
    public class ExamAttendancesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ExamAttendancesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPut("edit")]
        public async Task<IActionResult> EditAttendance([FromBody] EditExamAttendanceDTO attendance)
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
            ExamAttendance? toEdit = await _context.ExamAttendances.FirstOrDefaultAsync(c => c.Id == attendance.Id);
            if (toEdit == null)
                return NotFound(new { message = "AttendanceNotFound" });
            toEdit.Status = attendance.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "ExamAttendanceUpdated" });
        }

        [Authorize]
        [HttpGet("user/getall")]
        public async Task<IActionResult> GetAllUser()
        {
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            var examAttendances = await _context.ExamAttendances
                .Include(ea => ea.Exam)
                    .ThenInclude(e => e.Classroom)
                .Include(ea => ea.Exam)
                    .ThenInclude(e => e.Course)
                .Where(ea => ea.User.NeptunId == neptunId)
                .ToListAsync();

            return Ok(new
            {
                message = "Authorized",
                attendances = examAttendances
            });
        }
    }
}
