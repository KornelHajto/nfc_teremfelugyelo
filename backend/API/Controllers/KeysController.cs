using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeysController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KeysController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateKeycard([FromBody] AddNewKeycardDTO data)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            Key? key = await _context.Keys.FirstOrDefaultAsync(k => k.Hash == data.AdminKeycard);
            if (key == null) { return Unauthorized(new { message = "KeycardNotFound" }); }

            User? user = await _context.Users
            .Include(u => u.Keys)
            .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == data.AdminKeycard));
            if (user == null) { return Unauthorized(new { message = "UserNotFound" }); }
            if (user.AdminLevel != AdminLevels.Admin) { return Unauthorized(new { message = "NotAuthorized" }); }

            User? register = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId.ToUpper() == data.NeptunId);
            if (register == null) { return Unauthorized(new { message = "ToRegisterNotFound" }); }

            string uid = Guid.NewGuid().ToString("N").Substring(0, 16);
            register.Keys.Add(new Key()
            {
                Hash = uid,
            });
            await _context.SaveChangesAsync();

            return Ok(new {key = uid});
        }
    }
}
