using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StretchingStudioAPI.Data;
using StretchingStudioAPI.Models;
using StretchingStudioAPI.Models.Subscriptions;

namespace StretchingStudioAPI.Controllers;

[ApiController]
[Route("api/v1/[action]")]
public class SubscriptionsController : ControllerBase
{
    private readonly BookingServiceContext _bookingContext;
    private readonly AuthContext _authContext;
    
    public SubscriptionsController(BookingServiceContext bookingContext, AuthContext authContext)
    {
        _bookingContext = bookingContext;
        _authContext = authContext;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<UserSubscription>>> UserSubscriptions()
    {
        var user = await _authContext.Users.SingleOrDefaultAsync(u => u.Email == HttpContext.User.Identity!.Name);

        var userId = Guid.Parse(user!.Id);
        
        var subscriptions = await _bookingContext.UserSubscriptions
            .Include(s => s.Subscription)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.SessionsLeft)
            .ToListAsync();

        return Ok(subscriptions);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PurchaseSubscription([FromBody] PurchaseSubscription requestedSubscription)
    {
        var user = await _authContext.Users.SingleOrDefaultAsync(u => u.Email == HttpContext.User.Identity!.Name);
        
        var subscription = await _bookingContext.AvailableSubscriptions.SingleOrDefaultAsync(s => s.Id == requestedSubscription.SubscriptionId);

        if (subscription is null)
            return NotFound(new
            {
                message = "Subscription with the given ID wasn't found"
            });
        
        var newSubscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(user!.Id),
            Subscription = subscription,
            SessionsLeft = subscription.SessionsNum
        };
        
        await _bookingContext.UserSubscriptions.AddAsync(newSubscription);
        
        await _bookingContext.SaveChangesAsync();

        return Ok(newSubscription);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<AvailableSubscription>>> AvailableSubscriptions()
    {
        var subscriptions = await _bookingContext.AvailableSubscriptions
            .OrderBy(s => s.Price)
            .ToListAsync();
        
        return Ok(subscriptions);
    }
}