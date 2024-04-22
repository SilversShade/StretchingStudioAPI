using Microsoft.EntityFrameworkCore;
using StretchingStudioAPI.Models;

namespace StretchingStudioAPI.Data;

public class BookingServiceContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AvailableSubscription> AvailableSubscriptions { get; set; } = null!;

    public DbSet<BookedSession> BookedSessions { get; set; } = null!;

    public DbSet<SessionType> SessionTypes { get; set; } = null!;

    public DbSet<UpcomingSession> UpcomingSessions { get; set; } = null!;

    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;

    public DbSet<Schedule> Schedule { get; set; } = null!;
}