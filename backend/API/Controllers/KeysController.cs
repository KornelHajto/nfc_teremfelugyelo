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
                .Include(u => u.Courses)
                .ThenInclude(c => c.Classroom)
                .Include(u => u.Courses)
                .ThenInclude(c => c.Subject)
                .FirstOrDefaultAsync(u => u.Keys.Any(k => k.Hash == Keycard.Hash));
            if (user == null) { return Unauthorized(new { message = "KeycardNotFound" }); }
            Classroom? room = await _context.Classrooms.FirstOrDefaultAsync(c => c.RoomId == Keycard.RoomId);
            if (room == null) { return NotFound(new { message = "RoomNotFound" }); }
            bool isInClass = _context.Classrooms
                .Any(c => c.InRoom.Any(u => u.NeptunId == user.NeptunId));
            if (isInClass)
            {
                return BadRequest(new { message = "UserAlreadyInRoom" });
            }
           

            var now = DateTime.Now;

            // Check for an exam in progress in the room
            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Classroom)
                .FirstOrDefaultAsync(e =>
                    e.Classroom.RoomId == Keycard.RoomId &&
                    e.Date.Date == now.Date.Date &&
                    now <= e.Date + e.Duration);

            if (exam != null)
            {
                ExamAttendance? examA = await _context.ExamAttendances
                    .FirstOrDefaultAsync(ea => ea.User.NeptunId == user.NeptunId && ea.Exam.Id == exam.Id);

                if (examA != null)
                {
                    switch (examA.Status)
                    {
                        case ExamStatusTypes.Approved:
                            return Ok(new { message = "Authorized" });
                        case ExamStatusTypes.Denied:
                            return Unauthorized(new { message = "DeniedByTeacher" });
                        default:
                            return Unauthorized(new { message = "WaitForResponse" });
                    }
                }

                // Check if user is enrolled in the course
                bool isEnrolled = user.Courses.Any(c => c.Id == exam.Course.Id);
                if (!isEnrolled)
                {
                    return Unauthorized(new { message = "NotEnrolledInExamCourse" });
                }

                // Determine if user is late
                var examEnterDeadline = exam.Date + exam.EnterSpan;
                
                Console.WriteLine(now.ToLocalTime() +" > "+examEnterDeadline.ToLocalTime());
                var examStatus = now <= examEnterDeadline ? ExamStatusTypes.Approved : ExamStatusTypes.Waiting; ;

                // Add ExamAttendance
                ExamAttendance examAttendance = new()
                {
                    User = user,
                    Exam = exam,
                    Arrival = now,
                    Status = examStatus
                };
                await _context.ExamAttendances.AddAsync(examAttendance);
                await _context.SaveChangesAsync();

                return Ok(new { message = "ExamAttendanceRecorded", status = examStatus.ToString() });
            }

            // If no exam is in progress, check for a course
            var userCourse = user.Courses
                .FirstOrDefault(c => c.Classroom.RoomId == Keycard.RoomId && c.Date.Any(d => d.Date.Date == now.Date));
            if (userCourse == null)
            {
                return Unauthorized(new { message = "NoCourseInRoomToday" });
            }

            var scheduledDate = userCourse.Date.FirstOrDefault(d => d.Date == now.Date);
            if (scheduledDate == default)
            {
                return Unauthorized(new { message = "NoCourseScheduledToday" });
            }

            var courseEndTime = scheduledDate + userCourse.Duration;
            if (now > courseEndTime)
            {
                return Unauthorized(new { message = "CourseAlreadyEnded" });
            }

            var isLate = now > scheduledDate;
            var attendanceType = isLate ? AttendanceTypes.Late : AttendanceTypes.Arrived;

            room.InRoom.Add(user);

            if (userCourse.Subject == null)
            {
                return BadRequest(new { message = "CourseSubjectMissing" });
            }

            Attendance attendance = new()
            {
                User = user,
                Subject = userCourse.Subject,
                Arrival = now,
                AttendanceType = attendanceType,
                Comment = ""
            };
            await _context.Attendances.AddAsync(attendance);

            string newId = await ReplaceUID(key);
            Log log = new()
            {
                Date = now,
                User = user,
                Classroom = room,
                EnterType = EnterTypes.Enter
            };
            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();

            if (user.AdminLevel == AdminLevels.Admin)
            {
                return Ok(new { message = "AuthorizedAsAdmin", newUID = newId, attendanceType });
            }
            return Ok(new { message = "Authorized", newUID = newId, attendanceType });
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
            Classroom? room = await _context.Classrooms
                .Include(c => c.InRoom)
                .FirstOrDefaultAsync(c => c.InRoom.Any(u => u.NeptunId == user.NeptunId));
            if (room != null)
            {
                room.InRoom.RemoveAll(u => u.NeptunId == user.NeptunId);
                await _context.SaveChangesAsync();
            }
            Log log = new()
            {
                Date = DateTime.Now,
                User = user,
                Classroom = room,
                EnterType = EnterTypes.Exit
            };
            await _context.Logs.AddAsync(log);

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

        [HttpGet("tempkey")]
        public async Task<IActionResult> CreateTempKey()
        {
            if (!ModelState.IsValid) { return BadRequest(new { message = "InvalidForm" }); }
            var neptunId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(neptunId))
                return BadRequest(new { message = "NoId" });

            var userC = await _context.Users.FirstOrDefaultAsync(u => u.NeptunId == neptunId);
            if (userC == null)
                return Unauthorized(new { message = "NoUserFound" });

            string tempHash = Guid.NewGuid().ToString("N").Substring(0, 16);

            var tempKey = new Key
            {
                Hash = tempHash,
                Expiration = DateTime.UtcNow.AddMinutes(1)
            };

            userC.Keys.Add(tempKey);
            await _context.SaveChangesAsync();

            return Ok(new { key = tempHash, expiresAt = tempKey.Expiration });
        }
    }
}
