using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StretchingStudioAPI.Data;

public class AuthContext(DbContextOptions<AuthContext> options) : IdentityDbContext<IdentityUser>(options);