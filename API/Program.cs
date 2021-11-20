using API.ExceptionMiddleware;
using API.Extensions;
using API.Helpers;
using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
builder.Services.AddDbContext<StoreContext>(x => x.UseSqlServer(config.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfiles));
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerDocumentation();
var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();

    try
    {
        var context = builder.Services.BuildServiceProvider().GetRequiredService<StoreContext>();
        await context.Database.MigrateAsync();
        await StoreContextSeed.SeedAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occured while migration");
    }



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseSwaggerDocumentation();
app.UseAuthorization();

app.MapControllers();

app.Run();
