using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendancesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AttendancesController(AppDbContext context)
        {
            _context = context;
        }



    }
}
