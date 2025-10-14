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
        public async Task<IActionResult> CreateClassroom([FromBody] CreateClassroomDTO classroom)
        {
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if(neptunId == null) { return BadRequest(new { message = "NoId" }); }
            User? user = await _context.Users.FirstOrDefaultAsync(u => neptunId == u.NeptunId);
            if (user == null) { return Unauthorized(new { message = "NoUserFound" }); }
            if (user.AdminLevel != AdminLevels.Admin) { return Unauthorized(new { message = "NoPermission" }); }
            bool exists = await _context.Classrooms.AnyAsync(c => c.RoomId == classroom.Name);
            if (exists)
            {
                return Conflict(new { message = "ClassroomNameTaken" });
            }
            Classroom newClassroom = new() { RoomId = classroom.Name };
            await _context.Classrooms.AddAsync(newClassroom);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ClassroomCreated" });
        }
    }
}
