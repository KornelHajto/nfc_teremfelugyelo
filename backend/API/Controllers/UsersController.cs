using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public IActionResult CreateUser(string NeptunId, string Password)
        {
            User user = new()
            {
                NeptunId = NeptunId,
                Password = Password,
            };

            return Ok("USER CREATED");
        }
    }
}
