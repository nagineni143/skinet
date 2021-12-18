using API.ExceptionMiddleware;
using API.Extensions;
using API.Helpers;
using Core.Entities.Identity;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
builder.Services.AddDbContext<StoreContext>(x => x.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppIdentityDbContext>(x => x.UseSqlServer(config.GetConnectionString("IdentityConnection")));
builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
{
    var configuration = ConfigurationOptions.Parse(config.GetConnectionString("Redis"), true);
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddAutoMapper(typeof(MappingProfiles));
builder.Services.AddApplicationServices();
builder.Services.AddIdentityServices(config);
builder.Services.AddSwaggerDocumentation();
var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();

    try
    {
        var context = builder.Services.BuildServiceProvider().GetRequiredService<StoreContext>();
        await context.Database.MigrateAsync();
        await StoreContextSeed.SeedAsync(context, loggerFactory);
        var userManager = builder.Services.BuildServiceProvider().GetRequiredService<UserManager<AppUser>>();
        var identityContext = builder.Services.BuildServiceProvider().GetRequiredService<AppIdentityDbContext>();
        await identityContext.Database.MigrateAsync();
        await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occured while migration");
    }

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
     {
         policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
     });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
