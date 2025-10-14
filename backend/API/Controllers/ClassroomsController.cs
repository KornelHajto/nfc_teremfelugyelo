using API.Controllers;
using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ClassroomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClassroomsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateClassroom([FromBody] ClassroomDTO classroom)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if(neptunId == null) { return BadRequest(new { message = "NoId" }); }
            User? user = await _context.Users.FirstOrDefaultAsync(u => neptunId == u.NeptunId);
            if (user == null) { return Unauthorized(new { message = "NoUserFound" }); }
            if (user.AdminLevel != AdminLevels.Admin) { return Unauthorized(new { message = "NoPermission" }); }
            bool exists = await _context.Classrooms.AnyAsync(c => c.RoomId == classroom.RoomId);
            if (exists)
            {
                return Conflict(new { message = "ClassroomNameTaken" });
            }
            Classroom newClassroom = new() { RoomId = classroom.RoomId };
            await _context.Classrooms.AddAsync(newClassroom);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ClassroomCreated" });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteClassroom([FromBody] ClassroomDTO classroom)
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

            Classroom? roomToDel = await _context.Classrooms.FirstOrDefaultAsync(c => c.RoomId == classroom.RoomId);
            if (roomToDel == null)
            {
                return NotFound(new { message = "ClassroomNotFound" });
            }
            _context.Classrooms.Remove(roomToDel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ClassroomDeleted" });
        }

        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllClassrooms()
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
            List<Classroom> rooms = await _context.Classrooms.ToListAsync();

            return Ok(new { message = "Authorized", classrooms = rooms });

        }
    }
}
