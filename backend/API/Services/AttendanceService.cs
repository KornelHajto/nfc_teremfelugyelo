using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.Services
{
    public class AttendanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AttendanceService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.Now;

                    var todayCourses = (await db.Courses
                        .Include(c => c.Students)
                        .Include(c => c.Subject)
                        .ToListAsync(stoppingToken))
                        .Where(c => c.Date.Any(d => d.Date == now.Date))
                        .ToList();

                    foreach (var course in todayCourses)
                    {
                        var scheduledDate = course.Date.FirstOrDefault(d => d.Date == now.Date);
                        if (scheduledDate == default) continue;

                        var courseEndTime = scheduledDate + course.Duration;
                        if (now < courseEndTime) continue;

                        var attendedStudentIds = await db.Attendances
                            .Where(a => a.Subject.Id == course.Subject.Id && a.Arrival.Date == now.Date)
                            .Select(a => a.User.NeptunId)
                            .ToListAsync(stoppingToken);

                        foreach (var student in course.Students)
                        {
                            if (attendedStudentIds.Contains(student.NeptunId)) continue;

                            var absence = new Attendance
                            {
                                User = student,
                                Subject = course.Subject,
                                Arrival = scheduledDate,
                                AttendanceType = AttendanceTypes.AbsenceWithoutExcuse,
                                Comment = ""
                            };
                            db.Attendances.Add(absence);
                        }
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}