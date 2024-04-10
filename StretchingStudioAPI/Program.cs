using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StretchingStudioAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    // в проде заменить получение connection string на переменные окружения
    builder.Services.AddDbContext<AuthContext>(options =>
        options.UseNpgsql(builder.Configuration["AuthConnection"]));
}

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AuthContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<IdentityUser>();

app.UseHttpsRedirection();

app.Run();