using Interfaces;
using Services;
using Entities;
using Data;
using Repositories;
using Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Profiles;
using Hubs;
using GabineteDigital.Api;
using GabineteDigital.Application.Interfaces;
using GabineteDigital.Infrastructure.Services;
using Microsoft.EntityFrameworkCore; // Necessário para ClaimTypes

var builder = WebApplication.CreateBuilder(args);

// Configurar o DbContext
builder.Services.AddDbContext<GabineteDigitalDbContext>(options =>
{
    // Use PostgreSQL
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Ou Use SQL Server
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<MembroEquipe, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true; // Boas práticas de segurança
    options.Password.RequireLowercase = true;  // Boas práticas de segurança
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false; // Pode ser true para e-mail de confirmação
})
.AddEntityFrameworkStores<GabineteDigitalDbContext>()
.AddDefaultTokenProviders();

// Configurar JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
    };
    // Adicionar suporte para JWT em SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/notifications") || path.StartsWithSegments("/dashboard")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins(builder.Configuration["CorsOrigins"].Split(',')) // Domínio(s) do frontend Laravel
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()); // Necessário para SignalR e cookies se forem usados
});

// Adicionar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Injeção de Dependência dos Repositórios
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISolicitacaoRepository, SolicitacaoRepository>();
builder.Services.AddScoped<ICompromissoRepository, CompromissoRepository>();
// Adicione outros repositórios aqui

// Injeção de Dependência dos Serviços de Aplicação
builder.Services.AddScoped<ISolicitacaoService, SolicitacaoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAgendaService, AgendaService>();

// Configurar serviços de IA e Multicanal
builder.Services.AddHttpClient<IAIService, OpenAIService>(); // Exemplo: OpenAIService
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>(); // Ou LocalFileStorageService
builder.Services.AddScoped<IMultichannelService, TwilioService>(); // Crie TwilioService

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // Evitar ciclos de referência
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Gabinete Digital API", Version = "v1" });
    // Adicionar suporte a JWT no Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            },
            Array.Empty<string>()
        }
    });
});

// Adicionar SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting(); // Importante para o CORS funcionar antes de UseAuthentication

app.UseCors("CorsPolicy"); // Usar a política CORS definida

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notifications"); // Mapear SignalR Hubs
app.MapHub<DashboardHub>("/dashboard");       // Mapear SignalR Hubs

// Executar Migrações e Seed de dados (apenas para desenvolvimento/primeira execução)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GabineteDigitalDbContext>();
    // Aplica migrações pendentes. Use MigrateAsync() para async.
    // context.Database.Migrate();

    // Em produção, considere aplicar migrações como parte do seu pipeline de CI/CD
    // For Dev, uncomment:
    try
    {
        await dbContext.Database.MigrateAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MembroEquipe>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await SeedData.Initialize(dbContext, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        // Em produção, você pode querer re-throw ou lidar com isso de forma diferente.
    }
}


app.Run();