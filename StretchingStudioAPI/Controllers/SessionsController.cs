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

        var userSubscription = await _bookingContext.UserSubscriptions.FirstOrDefaultAsync();
        if (userSubscription is null || userSubscription.SessionsLeft == 0)
            return Conflict(new
            {
                noSubscriptionMessage = "User must own a valid subscription to sign up for a session."
            });
        
        var session = await _bookingContext.UpcomingSessions
            .Include(s => s.SessionType)
            .SingleOrDefaultAsync(s => s.Id == requestedSession.SessionId);
        
        if (session is null)
            return NotFound(new
            {
                noRequestedSessionMessage = "Session with requested ID wasn't found in upcoming sessions."
            });
        
        if (session.FreeSlotsNum == 0)
            return Conflict(new
            {
                noFreeSlotsMessage = "No slots available for the requested session."
            });
        
        if (session.StartingDate < DateTime.UtcNow + TimeSpan.FromHours(5))
            return Conflict(new
            {
                dateConflictMessage = "It's too late for signing up for this session."
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