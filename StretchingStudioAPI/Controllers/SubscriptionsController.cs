using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StretchingStudioAPI.Data;
using StretchingStudioAPI.Models;

namespace StretchingStudioAPI.Controllers;

[ApiController]
[Route("api/v1/[action]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ILogger<SubscriptionsController> _logger;
    private readonly BookingServiceContext _dbContext;
    
    public SubscriptionsController(ILogger<SubscriptionsController> logger, BookingServiceContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<AvailableSubscription>>> AvailableSubscriptions()
    {
        var subscriptions = await _dbContext.AvailableSubscriptions
            .OrderBy(s => s.Price)
            .ToListAsync();
        
        return Ok(subscriptions);
    }
}