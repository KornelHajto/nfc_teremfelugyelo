using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeysController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KeysController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<string> ReplaceUID(Key key)
        {
            string uid = Guid.NewGuid().ToString("N").Substring(0, 16);
            //key.Hash = uid;
            //await _context.SaveChangesAsync();
            return uid;
        }

        [HttpPost("enter")]
        public async Task<IActionResult> KeycardEnter([FromBody] KeycardEnterDTO Keycard)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            Key? key = await _context.Keys.FirstOrDefaultAsync(k => k.Hash == Keycard.Hash);
            if (key == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            User? user = await _context.Users
            .Include(u => u.Keys)
            .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == Keycard.Hash));
            if (user == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            Classroom? room = await _context.Classrooms.FirstOrDefaultAsync(c => c.RoomId == Keycard.RoomId);
            if(room == null) { return NotFound(new { message = "RoomNotFound" }); }
            room.InRoom.Add(user);

            string newId = await ReplaceUID(key);

            await _context.SaveChangesAsync();
            if (user.AdminLevel == AdminLevels.Admin) {
                return Ok(new { message = "AuthorizedAsAdmin", newUID = newId });
            }
            return Ok(new { message = "Authorized", newUID = newId });
        }

        [HttpPost("image")]
        public async Task<IActionResult> KeycardGetImage([FromBody] KeycardDTO Keycard)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            Key? key = await _context.Keys.FirstOrDefaultAsync(k => k.Hash == Keycard.Hash);
            if (key == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            User? user = await _context.Users
            .Include(u => u.Keys)
            .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == Keycard.Hash));
            if (user == null) { return Unauthorized(new { message = "KeycardNotFound" }); }

            return Ok(new { message = "Authorized", image = user.Picture});
        }

        [HttpPost("exit")]
        public async Task<IActionResult> KeycardExit([FromBody] KeycardDTO Keycard)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            Key? key = await _context.Keys.FirstOrDefaultAsync(k => k.Hash == Keycard.Hash);
            if (key == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            User? user = await _context.Users
            .Include(u => u.Keys)
            .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == Keycard.Hash));
            if (user == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            string newId = await ReplaceUID(key);

            if (user.AdminLevel == AdminLevels.Admin)
            {
                return Ok(new { message = "AuthorizedAsAdmin", newUID = newId });
            }
            return Ok(new { message = "Authorized", newUID = newId });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateKeycard([FromBody] AddNewKeycardDTO data)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            Key? key = await _context.Keys.FirstOrDefaultAsync(k => k.Hash == data.AdminKeycard);
            if (key == null) { return Unauthorized(new { message = "KeycardNotFound" }); }

            User? user = await _context.Users
            .Include(u => u.Keys)
            .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == data.AdminKeycard));
            if (user == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            if (user.AdminLevel != AdminLevels.Admin) { return Unauthorized(new { message = "NotAuthorized" }); }

            User? register = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId.ToUpper() == data.NeptunId);
            if (register == null) { return Unauthorized(new { message = "ToRegisterNotFound" }); }

            string uid = Guid.NewGuid().ToString("N").Substring(0, 16);
            register.Keys.Add(new Key()
            {
                Hash = uid,
            });
            await _context.SaveChangesAsync();

            return Ok(new {key = uid });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteKey([FromBody] KeycardDTO key)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var userC = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (userC == null)
                return Unauthorized(new { message = "NoUserFound" });

            if (userC.AdminLevel != AdminLevels.Admin)
                return Unauthorized(new { message = "NoPermission" });

            Key? keyToDel = await _context.Keys.FirstOrDefaultAsync(c => c.Hash == key.Hash);
            if (keyToDel == null)
            {
                return NotFound(new { message = "KeyNotFound" });
            }
            _context.Keys.Remove(keyToDel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "KeyDeleted" });
        }
    }
}
