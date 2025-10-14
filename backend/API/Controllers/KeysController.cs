using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
        public async Task<IActionResult> KeycardEntered([FromBody] KeycardDTO Keycard)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            Key? key = await _context.Keys.FirstOrDefaultAsync(k => k.Hash == Keycard.Hash);
            if (key == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            User? user = await _context.Users
            .Include(u => u.Keys)
            .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == Keycard.Hash));
            if (user == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            string newId = await ReplaceUID(key);

            if (user.AdminLevel == AdminLevels.Admin) {
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
    }
}
