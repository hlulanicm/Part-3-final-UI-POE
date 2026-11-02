using API_proj.Data;
using Microsoft.AspNetCore.Authentication.Cookies;   
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


   builder.Services.AddEndpointsApiExplorer();


 builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ApplicationDBContext>(options =>

{
    options.UseSqlServer(

        builder.Configuration.GetConnectionString("DefaultConnection"));

});

//  cookie authentication

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDayAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        options.SlidingExpiration = true;

        // Return status codes instead of redirecting 

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;

               return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
                 return Task.CompletedTask;
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();   
app.UseAuthorization();

app.MapControllers();

app.Run();