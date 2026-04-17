using System.Text;
using EAMS.Api.Auth;
using EAMS.Api.Data;
using EAMS.Application;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using EAMS.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EAMSDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("EAMS.Api")));

// 注册 DbContext 作为基类供仓储使用
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<EAMSDbContext>());

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "")),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS - 支持局域网访问
var allowAnyOrigin = builder.Configuration.GetValue<bool>("Cors:AllowAnyOrigin", true);
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowAnyOrigin)
        {
            // 局域网环境：允许任何来源
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else if (corsOrigins != null && corsOrigins.Length > 0)
        {
            // 生产环境：仅允许配置的来源
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EAMSDbContext>();
    db.Database.EnsureCreated();
    
    // Seed default roles - ensure they exist
    var defaultRoles = new[]
    {
        new SysRole { RoleCode = "admin", RoleName = "系统管理员", Description = "系统超级管理员，拥有所有权限", IsEnabled = true, SortOrder = 1 },
        new SysRole { RoleCode = "user", RoleName = "普通用户", Description = "普通用户，拥有基本操作权限", IsEnabled = true, SortOrder = 2 },
        new SysRole { RoleCode = "guest", RoleName = "访客", Description = "访客，仅查看权限", IsEnabled = true, SortOrder = 3 },
        new SysRole { RoleCode = "asset_admin", RoleName = "资产管理员", Description = "资产管理员，负责资产管理", IsEnabled = true, SortOrder = 4 }
    };
    
    foreach (var role in defaultRoles)
    {
        if (!db.SysRoles.Any(r => r.RoleCode == role.RoleCode))
        {
            db.SysRoles.Add(role);
        }
    }
    db.SaveChanges();
    
    // Seed default admin user if not exists
    if (!db.SysUsers.Any(u => u.Username == "admin"))
    {
        var adminRole = db.SysRoles.First(r => r.RoleCode == "admin");
        var userRole = db.SysRoles.First(r => r.RoleCode == "user");
        
        // Create admin user
        var adminUser = new SysUser
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            RealName = "系统管理员",
            Email = "admin@eams.com",
            Phone = "13800138000",
            IsEnabled = true
        };
        db.SysUsers.Add(adminUser);
        db.SaveChanges();
        
        // Create test user
        var testUser = new SysUser
        {
            Username = "test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
            RealName = "测试用户",
            Email = "test@eams.com",
            Phone = "13900139000",
            IsEnabled = true
        };
        db.SysUsers.Add(testUser);
        db.SaveChanges();
        
        // Assign roles
        db.SysUserRoles.Add(new SysUserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        db.SysUserRoles.Add(new SysUserRole { UserId = testUser.Id, RoleId = userRole.Id });
        db.SaveChanges();
        
        Console.WriteLine("=== Default users created ===");
        Console.WriteLine("Admin - Username: admin, Password: admin123");
        Console.WriteLine("Test  - Username: test, Password: test123");
        Console.WriteLine("==================================");
    }
}

// Use CORS before auth
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
