using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDTO course)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            if (user.AdminLevel != AdminLevels.Admin)
                return Unauthorized(new { message = "NoPermission" });

            bool exists = await _context.Courses.AnyAsync(c => c.Name == course.Name);
            if (exists)
            {
                return Conflict(new { message = "CourseNameTaken" });
            }
            Subject? subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == course.SubjectId);
            if (subject == null) { return NotFound(new { message = "SubjectNotFound" }); }

            Classroom? classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.RoomId == course.ClassroomId);
            if (classroom == null) { return NotFound(new { message = "ClassroomNotFound" }); }
            Course newCourse = new()
            {
                Name = subject.Name,
                Subject = subject,
                Classroom = classroom,
                Duration = course.Duration
            };
            await _context.Courses.AddAsync(newCourse);
            await _context.SaveChangesAsync();

            return Ok(new { message = "CourseCreated" });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCourse([FromBody] DeleteCourseDTO course)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            if (user.AdminLevel != AdminLevels.Admin)
                return Unauthorized(new { message = "NoPermission" });

            Course? courseToDel = await _context.Courses.FirstOrDefaultAsync(c => c.Id == course.Id);
            if (courseToDel == null)
            {
                return NotFound(new { message = "CourseNotFound" });
            }
            _context.Courses.Remove(courseToDel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "CourseDeleted" });
        }

        [Authorize]
        [HttpPost("dates")]
        public async Task<IActionResult> AddCourseDates([FromBody] AddCourseDatesDTO course)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            if (user.AdminLevel == AdminLevels.Student)
                return Unauthorized(new { message = "NoPermission" });
            Course? courseToUpdate = await _context.Courses.FirstOrDefaultAsync(c => c.Id == course.Id);
            if (courseToUpdate == null)
            {
                return NotFound(new { message = "CourseNotFound" });
            }
            DateTime startDate = course.StartDate;
            DateTime endDate = course.EndDate;
            while (startDate <= endDate)
            {
                courseToUpdate.Date.Add(startDate);
                startDate = startDate.AddDays(7);
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "CourseDatesAdded" });
        }

        [Authorize]
        [HttpGet("user/getall")]
        public async Task<IActionResult> UserGetAll()
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            return Ok(new { message = "Authorized", courses = user.Courses });
        }

        [Authorize]
        [HttpPost("user/add")]
        public async Task<IActionResult> UserAddCourse([FromBody] AddCourseDTO courseDto)
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
            User? student = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == courseDto.NeptunId);
            if (student == null)
                return Unauthorized(new { message = "NoStudentFound" });

            Course? course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseDto.Id);
            if (course == null)
                return NotFound(new { message = "CourseNotFound" });

            student.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "CourseAddedToUser" });
        }

        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllCourses()
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
            List<Course> courses = await _context.Courses.ToListAsync();

            return Ok(new { message = "Authorized", courses = courses });

        }
    }
}
