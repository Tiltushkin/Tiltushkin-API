using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Api.Data;
using MyApp.Api.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Load env vars as overrides of appsettings
builder.Configuration.AddEnvironmentVariables();

// ===== EF Core + MySQL =====
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
var serverVersion = ServerVersion.AutoDetect(connStr);

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseMySql(connStr, serverVersion, o => o.EnableRetryOnFailure());
});

// ===== Identity (Users/Roles) =====
builder.Services
    .AddIdentityCore<IdentityUser>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequiredLength = 8;
        o.Password.RequireDigit = true;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

// ===== JWT Auth =====
var jwtOpts = new JwtOptions
{
    Issuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["Jwt:Issuer"] ?? "MyApp",
    Audience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["Jwt:Audience"] ?? "MyAppAudience",
    Secret = builder.Configuration["JWT_SECRET"] ?? builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Missing JWT secret"),
    ExpMinutes = 60
};

builder.Services.Configure<JwtOptions>(o =>
{
    o.Issuer = jwtOpts.Issuer;
    o.Audience = jwtOpts.Audience;
    o.Secret = jwtOpts.Secret;
    o.ExpMinutes = jwtOpts.ExpMinutes;
});

builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtOpts.Issuer,
        ValidAudience = jwtOpts.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Secret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

// ===== CORS =====
var originsCsv = builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? "";
var allowedOrigins = originsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        if (allowedOrigins.Length == 0)
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // DEV fallback
        else
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

// ===== Controllers & Swagger =====
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Enable JWT in Swagger UI
    c.SwaggerDoc("v1", new() { Title = "MyApp API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new List<string>()
        }
    });
});

// ===== Rate Limiting (simple global) =====
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("fixed", o =>
    {
        o.PermitLimit = 100; // 100 req per 10s per IP
        o.Window = TimeSpan.FromSeconds(10);
        o.QueueLimit = 0;
    });
});

// ===== Health checks =====
builder.Services.AddHealthChecks();

var app = builder.Build();

// Create DB & seed roles/admin on startup (simple quick-start approach)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync(); // For quick start. For prod, switch to Migrate().

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("user")) await roleManager.CreateAsync(new IdentityRole("user"));
    if (!await roleManager.RoleExistsAsync("admin")) await roleManager.CreateAsync(new IdentityRole("admin"));

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var adminEmail = builder.Configuration["ADMIN_EMAIL"];
    var adminUserName = builder.Configuration["ADMIN_USERNAME"] ?? adminEmail;
    var adminPassword = builder.Configuration["ADMIN_PASSWORD"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing == null)
        {
            var admin = new IdentityUser { Email = adminEmail, UserName = adminUserName };
            var created = await userManager.CreateAsync(admin, adminPassword);
            if (created.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, new[] { "admin", "user" });
            }
        }
    }
}

// ===== Middlewares =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Simple security headers (be mindful of CSP + Swagger in dev)
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    // CSP below is minimal and permissive for Swagger; tighten for production SPA origins
    ctx.Response.Headers["Content-Security-Policy"] = "default-src 'self' 'unsafe-inline' 'unsafe-eval' data:";
    await next();
});

app.UseHttpsRedirection();
app.UseCors("default");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
