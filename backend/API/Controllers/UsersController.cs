using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

//GetAll users, subjects, courses, classrooms, attendances
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDTO UserForm)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            if (UserForm.Password.Length < 8 )
            {
                return Conflict(new { message = "PasswordTooShort" });
            }
            if (!(UserForm.Password.Any(char.IsDigit) && UserForm.Password.Any(char.IsLetter) && UserForm.Password.Any(char.IsUpper)))
            {
                return Conflict(new { message = "PasswordWrongChars" });
            }

            bool TakenAccount = _context.Users.Any(u => u.NeptunId == UserForm.NeptunId);
            if (TakenAccount) {
                return Conflict(new { message = "TakenNeptunId" });
            }

            string PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(UserForm.Password, 13);
            User user = new()
            {
                NeptunId = UserForm.NeptunId.ToUpper(),
                FullName = UserForm.FullName,
                Password = PasswordHash,
                Picture = UserForm.Picture,
            };
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "UserCreated"});
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDTO UserForm)
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            User? user = await _context.Users.FirstOrDefaultAsync(u => UserForm.NeptunId.ToUpper() == u.NeptunId);
            if (user == null)
            {
                return Unauthorized(new { message = "WrongUsernameOrPassword" });
            }
            if (!BCrypt.Net.BCrypt.EnhancedVerify(UserForm.Password, user.Password))
            {
                return Unauthorized(new {message = "WrongUsernameOrPassword"});
            }
            var claims = new[]
            {   
                new Claim(ClaimTypes.Name, user.NeptunId),
            };
            var jwtKey = _configuration["JwtKey"];
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );
            var WroteToken = new JwtSecurityTokenHandler().WriteToken(token);
            //user.RememberMe.Add(new RememberMe() { RememberHash = WroteToken});
            //await _context.SaveChangesAsync();
            Response.Headers.Append("Authorization", $"Bearer {WroteToken}");
            return Ok(new {message = "LoginSuccesful", token = WroteToken });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserDTO user)
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

            User? userToDel = await _context.Users.FirstOrDefaultAsync(c => c.NeptunId == user.NeptunId);
            if (userToDel == null)
            {
                return NotFound(new { message = "UserNotFound" });
            }
            _context.Users.Remove(userToDel);
            await _context.SaveChangesAsync();

            return Ok(new { message = "UserDeleted" });
        }

        [Authorize]
        [HttpGet("adminlevel")]
        public async Task<IActionResult> GetAdminLevel()
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (user == null)
                return Unauthorized(new { message = "NoUserFound" });

            return Ok(new { message = "Authorized", level = user.AdminLevel });
        }

        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUsers()
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
            List<User> users = await _context.Users.ToListAsync();

            return Ok(new { message = "Authorized", users = users });

        }
    }
}
