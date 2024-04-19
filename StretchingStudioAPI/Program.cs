using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using StretchingStudioAPI.Data;
using StretchingStudioAPI.Middleware.Transformers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    // в проде заменить получение connection string на переменные окружения
    builder.Services.AddDbContext<AuthContext>(options =>
        options.UseNpgsql(builder.Configuration["AuthConnection"]));

    builder.Services.AddDbContext<BookingServiceContext>(options => 
        options.UseNpgsql(builder.Configuration["BookingServiceConnection"]));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}

builder.Services.AddControllers(options =>
    options.Conventions.Add(new RouteTokenTransformerConvention(new ToKebabParameterTransformer())));
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AuthContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<IdentityUser>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();