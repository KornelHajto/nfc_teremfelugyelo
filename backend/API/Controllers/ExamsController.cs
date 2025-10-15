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
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDTO examDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "InvalidForm" });

            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            if (user.AdminLevel != AdminLevels.Admin && user.AdminLevel != AdminLevels.Teacher)
                return Unauthorized(new { message = "NoPermission" });

            var course = await _context.Courses.Include(c => c.Classroom).FirstOrDefaultAsync(c => c.Id == examDto.CourseId);
            if (course == null)
                return NotFound(new { message = "CourseNotFound" });

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.RoomId == examDto.ClassroomRoomId);
            if (classroom == null)
                return NotFound(new { message = "ClassroomNotFound" });

            Exam newExam = new()
            {
                Course = course,
                Classroom = classroom,
                Date = examDto.Date,
                Duration = examDto.Duration,
                EnterSpan = examDto.EnterSpan,
                ExitSpan = examDto.ExitSpan
            };

            await _context.Exams.AddAsync(newExam);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ExamCreated" });
        }
    }
}
