using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmacyMS.API.Exceptions;
using PharmacyMS.API.Auditing;
using PharmacyMS.API.Security;
using PharmacyMS.Application;
using PharmacyMS.Infrastructure;
using PharmacyMS.Infrastructure.Data;
using PharmacyMS.Infrastructure.Identity;
using PharmacyMS.Infrastructure.Security;

var builder=WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>{
    c.SwaggerDoc("v1",new OpenApiInfo{Title="PharmacyMS API",Version="v1"});
    c.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme{
        Name="Authorization",Type=SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT",In=ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {new OpenApiSecurityScheme{Reference=new OpenApiReference{Type=ReferenceType.SecurityScheme,Id="Bearer"}},Array.Empty<string>()}
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var key=builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key)<32)
    throw new InvalidOperationException("Configure Jwt:Key (at least 32 bytes) using dotnet user-secrets or environment variables.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>{
    o.TokenValidationParameters=new TokenValidationParameters{
        ValidateIssuer=true,ValidIssuer=builder.Configuration["Jwt:Issuer"],
        ValidateAudience=true,ValidAudience=builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(30),
        NameClaimType=ClaimTypes.Email,RoleClaimType=ClaimTypes.Role
    };
    o.Events=new JwtBearerEvents{
        OnTokenValidated=async context=>{
            var id=context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var users=context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user=id is null ? null : await users.FindByIdAsync(id);
            if (user is null || !user.IsActive || !user.EmailConfirmed || await users.IsLockedOutAsync(user)
                || context.Principal?.FindFirst("security_stamp")?.Value!=await users.GetSecurityStampAsync(user))
                context.Fail("Account is disabled or token is no longer valid.");
        }
    };
});
builder.Services.AddScoped<IAuthorizationHandler,PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options=>{
    options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach(var permission in PermissionNames.All)
        options.AddPolicy(permission,p=>p.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission)));
});
var origins=builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options=>options.AddPolicy("Angular",p=>{
    if (origins.Length>0) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

var app=builder.Build();
app.UseExceptionHandler();
if(app.Environment.IsDevelopment()) {app.UseSwagger();app.UseSwaggerUI();}
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditTrailMiddleware>();
app.MapControllers();
app.MapGet("/api/health",()=>Results.Ok(new{status="ok",databaseMode="EF Core Code-First"})).AllowAnonymous();
app.MapGet("/api/health/ready",async(ApplicationDbContext db,CancellationToken ct)=>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new{database="ready"}) : Results.Problem("Database unavailable",statusCode:503)).AllowAnonymous();

// No EnsureCreated/Migrate on startup. Use EF migrations explicitly (see SETUP_BN.md).
//if(app.Environment.IsDevelopment())
//    await AdminBootstrap.EnsureAsync(app.Services,app.Configuration);
// Allow one-time Admin Bootstrap in Production only when explicitly enabled.
var runProductionBootstrap =
    app.Environment.IsProduction() &&
    app.Configuration.GetValue<bool>("Bootstrap:RunOnce");

if (app.Environment.IsDevelopment() || runProductionBootstrap)
{
    app.Logger.LogInformation("Admin bootstrap execution started.");

    await AdminBootstrap.EnsureAsync(
        app.Services,
        app.Configuration);

    app.Logger.LogInformation("Admin bootstrap execution completed.");
}

app.Run();
public partial class Program { }
