using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartBook.Application.Helpers;
using SmartBook.Application.Interface;
using SmartBook.Application.Options;
using SmartBook.Application.Services;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Enums;
using SmartBook.Persistence.DbContexts; 
using SmartBook.Persistence.Repositories;
using SmartBook.Persistence.Repositories.Interface;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//BUSCAR QUE HACE
builder.Services.AddHttpContextAccessor();  // ← ESTO FALTA


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//Investigar que ahce a lujo de detalle
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SmartBook API",
        Version = "v1"
    });

    // ✨ CONFIGURACIÓN PARA JWT EN SWAGGER
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token JWT en el formato: Bearer: tu token"
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
            new string[] {}
        }
    });
});


//Registro mis dependencias en el contenedor de dependencias

var connectionString = builder.Configuration.GetConnectionString("SmartBook");

builder.Services.AddDbContext<SmartBookDbContext>(opt => opt.UseMySQL(connectionString));

// ⬇️ AGREGAR ESTO (configuración SMTP)
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.Configure<CorreosInstitucionalesOption>(builder.Configuration.GetSection("CorreosInstitucionalesOption"));


builder.Services.AddScoped<IUsuarioRepository, UsuarioEfcRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<IClienteRepository, ClienteEfcRepository>();

// Registrar Servicios
builder.Services.AddScoped<ClienteService>();



builder.Services.AddScoped<ILibroRepository, LibroEfcRepository>();
builder.Services.AddScoped<LibroService>();


builder.Services.AddScoped<IVentaRepository, VentaEfcRepository>();
builder.Services.AddScoped<VentaService>();


builder.Services.AddScoped<IIngresoRepository, IngresoEfcRepository>();
builder.Services.AddScoped<IngresoService>();



//investigar lo que es una inyeccion de dependencia
builder.Services.Configure<JwtOption>(builder.Configuration.GetSection("Jwt"));
/**
 Registra JwtOption en el sistema de inyección de dependencias
Lee la sección "Jwt" del appsettings.json
Mapea los valores a la clase JwtOption
¿Para qué sirve?
Si quieres inyectar JwtOption en otros lugares:
 */
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOption>();
/**
¿Qué hace?

Lee la sección "Jwt" del appsettings.json
Convierte el JSON a un objeto JwtOption
Lo guarda en la variable jwtSettings
**/

//builder.Configuration        // Lee appsettings.json
//    .GetSection("Jwt")       // Busca la sección "Jwt"
//    .Get<JwtOption>();       // Convierte a objeto JwtOption

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

//Convierte la SecretKey (string) a bytes
//Los tokens JWT necesitan una clave en formato byte[]

// ========== CONFIGURAR JWT ==========


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // En producción cambiar a true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Sin tiempo de gracia
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<SmartBookDbContext>();

        // Verificar si ya existe el admin
        var adminExiste = context.Usuarios.Any(u => u.IdentificacionUsuario == "ADMIN001");

        if (!adminExiste)
        {
            // Crear el usuario admin
            var usuarioAdmin = new Usuario
            {
                IdUsuario = DateTime.Now.Ticks.ToString(),
                IdentificacionUsuario = "ADMIN001",
                NombresUsuario = "Administrador CDI",
                ContraseniaUSuario = PasswordHelper.Encriptar("Admin123"),
                CorreoUsuario = "admin@cecar.edu.co", // ← Cambia por tu dominio
                RolUsuario = RolUsuario.Admin, // ← Admin
                EstadoUsuario = EstadoUsuario.Activo,
                FechaCreacion = DateTime.Now,
                EmailConfirmado = true, // ← Ya confirmado
                TokenConfirmacion = null,
                TokenExpiracion = null
            };

            context.Usuarios.Add(usuarioAdmin);
            context.SaveChanges(); // ← Esto guarda en la BD

            Console.WriteLine(" Usuario administrador creado");
            Console.WriteLine("   Usuario: ADMIN001");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(" Error al crear admin: {ex.Message}");
    }
}




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();  // ← DEBE ir ANTES de UseAuthorization() //lo unico que alñadi para el registro
app.UseAuthorization();

app.MapControllers();

app.Run();
