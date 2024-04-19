using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StretchingStudioAPI.Data;
using StretchingStudioAPI.Models;
using StretchingStudioAPI.Models.Sessions;

namespace StretchingStudioAPI.Controllers;

[ApiController]
[Route("api/v1/[action]")]
public class SessionsController : ControllerBase
{
    private readonly BookingServiceContext _bookingContext;
    private readonly AuthContext _authContext;
    
    public SessionsController(BookingServiceContext bookingContext, AuthContext authContext)
    {
        _bookingContext = bookingContext;
        _authContext = authContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<BookedSession>>> UserSessions()
    {
        var user = await _authContext.Users.SingleOrDefaultAsync(u => u.Email == HttpContext.User.Identity!.Name);

        var userSessions = await _bookingContext.BookedSessions
            .Where(s => s.UserId == Guid.Parse(user!.Id))
            .Include(s => s.Session)
            .Include(s => s.Session.SessionType)
            .ToListAsync();

        return Ok(userSessions
            .OrderBy(s => s.Session.StartingDate)
            .Where(s => s.Session.StartingDate > DateTime.UtcNow + TimeSpan.FromHours(5)));
    }
    
    [HttpGet]
    public async Task<ActionResult<List<UpcomingSession>>> UpcomingSessions(
        [FromQuery(Name = "free-slots-only")] bool freeSlotsOnly)
    {
        var sessions = await _bookingContext.UpcomingSessions
            .Where(s => s.StartingDate > DateTime.UtcNow + TimeSpan.FromHours(5))
            .Include(s => s.SessionType)
            .OrderBy(s => s.StartingDate)
            .ToListAsync();

        return freeSlotsOnly ? sessions.Where(s => s.FreeSlotsNum > 0).ToList() : sessions;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SessionSignUp([FromBody] SessionSignUp requestedSession)
    {
        var user = await _authContext.Users.SingleOrDefaultAsync(u => u.Email == HttpContext.User.Identity!.Name);

        var userSubscription = await _bookingContext.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == Guid.Parse(user!.Id));
        if (userSubscription is null || userSubscription.SessionsLeft == 0)
            return Conflict(new
            {
                message = "Необходимо приобрести абонемент для записи на тренировку."
            });
        
        var session = await _bookingContext.UpcomingSessions
            .Include(s => s.SessionType)
            .SingleOrDefaultAsync(s => s.Id == requestedSession.SessionId);
        
        if (session is null)
            return NotFound(new
            {
                message = "Запрашиваемая тренировка не была найдена."
            });
        
        if (session.FreeSlotsNum == 0)
            return Conflict(new
            {
                message = "На выбранную тренировку не осталось свободных мест."
            });
        
        if (session.StartingDate < DateTime.UtcNow + TimeSpan.FromHours(5))
            return Conflict(new
            {
                message = "Время для записи на выбранную тренировку истекло."
            });

        var duplicateBooking = await _bookingContext.BookedSessions
            .SingleOrDefaultAsync(s =>
                s.UserId == Guid.Parse(user!.Id) && s.Session.Id == requestedSession.SessionId);
        
        if (duplicateBooking is not null)
            return Conflict(new
            {
                message = "Вы уже записаны на данную тренировку."
            });
        
        var bookedSession = new BookedSession
        {
            Id = Guid.NewGuid(),
            Session = session,
            UserId = Guid.Parse(user!.Id)
        };

        await _bookingContext.BookedSessions.AddAsync(bookedSession);
        
        userSubscription.SessionsLeft--;
        if (userSubscription.SessionsLeft == 0)
            _bookingContext.UserSubscriptions.Remove(userSubscription);
        
        session.FreeSlotsNum--;
        
        await _bookingContext.SaveChangesAsync();

        return Ok(bookedSession);
    }
}