using ERP.API.Hubs;
using ERP.API.Middleware;
using ERP.API.Services;
using ERP.API.Validators;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using ERP.Core.Mappings;
using ERP.Data;
using ERP.Data.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("UniversalERP API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration));

    // 1. VERİTABANI
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 2. IDENTITY
    builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // 3. JWT
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // SignalR WebSocket bağlantıları token'ı query string'de gönderir
        // (Authorization header kullanamaz), bu yüzden burada özel olarak okuyoruz.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    // 4. AUTOMAPPER
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // 5. FLUENTVALIDATION
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

    // 6. ALTYAPI SERVİSLERİ
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<ITenantProvider, TenantProvider>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // 7. UYGULAMA SERVİSLERİ
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<ISaleService, SaleService>();
    builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
    builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IReportService, ReportService>();

    // 8. SIGNALR
    builder.Services.AddSignalR();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "UniversalERP API",
            Version = "v1",
            Description = "Multi-Tenant ERP Sistemi — Test kullanıcısı: master@erp.com / Master123!"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT token giriniz. Örnek: Bearer eyJhbGci..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (System.IO.File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy("AllowAll", policy =>
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    });

    var app = builder.Build();

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<ErpHub>("/hubs/erp");

    // SEED DATA
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "SuperAdmin", "TenantAdmin", "Manager", "Employee", "Cashier", "Technician" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var mainTenant = context.Tenants.IgnoreQueryFilters()
            .FirstOrDefault(t => t.Name == "Sistem Yönetim Merkezi");
        if (mainTenant == null)
        {
            mainTenant = new Tenant
            {
                Name = "Sistem Yönetim Merkezi",
                PlanType = "SaaS Admin",
                Industry = "Sistem",
                TaxNumber = "0000000000",
                Address = "Merkez Ofis / İstanbul",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            context.Tenants.Add(mainTenant);
            context.SaveChanges();
        }

        var seedTenants = new List<Tenant>
        {
            new() { Name = "Şehir Marketleri A.Ş.", Industry = "Market", TaxNumber = "10101", Address = "İstanbul", PlanType = "Gold", IsActive = true, CreatedAt = DateTime.Now },
            new() { Name = "Hızlı Tamir Merkezi", Industry = "Teknik Servis", TaxNumber = "20201", Address = "Eskişehir", PlanType = "Premium", IsActive = true, CreatedAt = DateTime.Now },
            new() { Name = "Siber Yazılım Evi", Industry = "Yazılım", TaxNumber = "30301", Address = "ODTÜ Teknokent", PlanType = "Premium", IsActive = true, CreatedAt = DateTime.Now },
        };
        foreach (var st in seedTenants)
        {
            if (!context.Tenants.IgnoreQueryFilters().Any(x => x.Name == st.Name))
                context.Tenants.Add(st);
        }
        context.SaveChanges();

        var adminEmail = "master@erp.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Sistem Sahibi",
                TenantId = mainTenant.Id,
                EmailConfirmed = true,
                Department = "Yönetim",
                Salary = 0,
                Role = "SuperAdmin"
            };
            var result = await userManager.CreateAsync(adminUser, "Master123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
        }
    }

    Log.Information("UniversalERP API başarıyla başlatıldı.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "UniversalERP API başlatılamadı!");
}
finally
{
    Log.CloseAndFlush();
}