using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManagerAPI.Data;
using TaskManagerAPI.Services;

// WebApplication.CreateBuilder sets up the app's configuration system,
// logging, and dependency injection container.
var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// REGISTER SERVICES (Dependency Injection Container)
// Think of this as a catalogue: "when someone asks for X, give them Y"
// ─────────────────────────────────────────────────────────────

// Add controller support - scans for classes that end in "Controller"
builder.Services.AddControllers();

// CORS (Cross-Origin Resource Sharing) — browsers block requests from one
// origin (http://localhost:5173) to another (http://localhost:5000) by default.
// This tells ASP.NET Core to allow requests from our React frontend.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite's default dev server port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register our custom AuthService.
// AddScoped means: create ONE instance per HTTP request, then throw it away.
// (Other options: AddSingleton = one instance ever, AddTransient = new instance every time it's injected)
builder.Services.AddScoped<IAuthService, AuthService>();

// Register AppDbContext with SQLite.
// builder.Configuration reads from appsettings.json automatically.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────────────────────────
// CONFIGURE JWT AUTHENTICATION
// ─────────────────────────────────────────────────────────────

// Read JWT settings from appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"]!;

builder.Services.AddAuthentication(options =>
{
    // Set JWT Bearer as the default authentication scheme.
    // This means: by default, look for a "Bearer token" in the Authorization header.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // TokenValidationParameters tells ASP.NET Core HOW to validate incoming tokens
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Check that the token's "iss" (issuer) matches ours
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Check that the token's "aud" (audience) matches ours
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Check that the token hasn't expired
        ValidateLifetime = true,

        // Verify the digital signature using our secret key
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

        // By default, ASP.NET adds 5 minutes of clock skew tolerance.
        // We set it to zero for precision.
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization allows us to use [Authorize] on controllers/actions
builder.Services.AddAuthorization();

// ─────────────────────────────────────────────────────────────
// CONFIGURE SWAGGER
// Swagger generates a nice browser UI for testing your API
// ─────────────────────────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1",
        Description = "A simple task manager API with JWT authentication"
    });

    // This section adds the "Authorize" button to the Swagger UI.
    // SecuritySchemeType.Http + Scheme "bearer" means Swagger automatically
    // adds the "Bearer " prefix — users just paste the raw token, nothing else.
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT token below (just the token, no 'Bearer' prefix needed)",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,  // Http type handles "Bearer " prefix automatically
        Scheme = "bearer",               // lowercase "bearer" is required for Http type
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Build the WebApplication object from all the registrations above
var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// CONFIGURE THE MIDDLEWARE PIPELINE
// Middleware runs on EVERY request, in order from top to bottom.
// Think of it as a series of security checkpoints.
// ─────────────────────────────────────────────────────────────

// Only show Swagger in development (not in production for security)
if (app.Environment.IsDevelopment())
{
    // Generates the swagger.json file (the API spec)
    app.UseSwagger();

    // Serves the browser UI at /swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API v1");
        // Make Swagger the default page when you open the browser
        c.RoutePrefix = string.Empty;
    });
}

// Redirects HTTP to HTTPS
app.UseHttpsRedirection();

// Apply the CORS policy — MUST come before Authentication and Authorization
app.UseCors("AllowReact");

// UseAuthentication: reads the JWT token from the Authorization header
// and populates User.Claims if the token is valid.
// MUST come BEFORE UseAuthorization.
app.UseAuthentication();

// UseAuthorization: checks if the authenticated user has permission
// to access the requested endpoint (respects [Authorize] attributes).
app.UseAuthorization();

// Maps controller routes: tells ASP.NET Core to look at all Controller
// classes and route requests to them based on [Route] and [Http*] attributes.
app.MapControllers();

// Run database migrations automatically on startup.
// This is convenient for development - in production you'd run migrations manually.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Start the web server and begin listening for requests
app.Run();
