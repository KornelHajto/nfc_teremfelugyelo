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

        [Authorize]
        [HttpGet("user/getall")]
        public async Task<IActionResult> UserGetAllExams()
        {
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            // Get all exams for the user's courses
            var userCourseIds = user.Courses.Select(c => c.Id).ToList();
            var examsList = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Classroom)
                .Where(e => userCourseIds.Contains(e.Course.Id))
                .Select(e => new
                {
                    e.Id,
                    e.Date,
                    e.Duration,
                    Course = new { e.Course.Id, e.Course.Name },
                    Classroom = new { e.Classroom.RoomId }
                })
                .ToListAsync();
            Console.WriteLine(userCourseIds);

            return Ok(new { message = "Authorized", exams = examsList });
        }

        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllExams()
        {
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });
            if (user.AdminLevel == AdminLevels.Student)
                return Unauthorized(new { message = "NotAuthorized" });

            var examList = await _context.Exams.ToListAsync();

            return Ok(new { message = "Authorized", attendances = examList });
        }
    }
}
