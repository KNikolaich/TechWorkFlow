using Backend.Jobs;
using Backend.Services;
using Backend.Infrastructure;
using System.Text;
using Common.Security;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<AdminBootstrapSettings>(builder.Configuration.GetSection(AdminBootstrapSettings.SectionName));
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddIdentityCore<Shared.Domain.ApplicationUser>(options =>
    {
        //options.Password.RequireDigit = true;
        //options.Password.RequireUppercase = true;
        //options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 6;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings are not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(AuthConstants.AdminRole));
    options.AddPolicy("SupervisorOnly", p => p.RequireRole(AuthConstants.SupervisorRole, AuthConstants.AdminRole));
    options.AddPolicy("WorkerOnly", p => p.RequireRole(AuthConstants.WorkerRole));
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskHistoryService, TaskHistoryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddQuartz(options =>
{
    var jobKey = new Quartz.JobKey(nameof(TaskHistoryCleanupJob));
    options.AddJob<TaskHistoryCleanupJob>(cfg => cfg.WithIdentity(jobKey));
    options.AddTrigger(cfg => cfg
        .ForJob(jobKey)
        .WithIdentity($"{nameof(TaskHistoryCleanupJob)}-trigger")
        .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(24).RepeatForever()));
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
