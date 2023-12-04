using ITDeskServer.Context;
using ITDeskServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ITDeskServer.Middleware;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


#region Authentication
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true, //Tokeni g�nderen ki�i bilgisi
        ValidateAudience = true, //Tokeni kullanacak site ya da ki�i bilgisi
        ValidateIssuerSigningKey = true, //Tokenin do�rulu�unu do�rulayacak anahtar
        ValidateLifetime = true, // Tokenin ya�am s�resini kontrol etmek istiyor musunuz?
        ValidIssuer = "Cagla Tunc Savas", //Tokeni g�nderen ki�i bilgisi
        ValidAudience = "IT Desk Angular App", //Tokeni kullanacak site ya da ki�i bilgisi
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my secret key my secret key my secret key 1234..."))//Tokenin do�rulu�unu do�rulayacak anahtar
    };
});
#endregion

#region DbContext ve Identity
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer("Data Source=CAGLA\\SQLEXPRESS;Initial Catalog=ITDeskDbWithWebApi;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
});

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequiredLength = 6;
    opt.SignIn.RequireConfirmedEmail = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opt.Lockout.MaxFailedAccessAttempts = 2;
    opt.Lockout.AllowedForNewUsers = true;

})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
#endregion

#region Presentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion


#region Middlewares
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

ExtensionsMiddleware.AutoMigration(app);

ExtensionsMiddleware.CreateFirstUser(app);

app.Run();
#endregion

