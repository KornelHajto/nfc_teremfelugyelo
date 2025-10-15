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
    }
}
