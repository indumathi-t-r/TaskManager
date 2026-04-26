using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Services
{
    // IAuthService is an INTERFACE - it defines the "contract" (what methods exist).
    // AuthService is the actual IMPLEMENTATION.
    //
    // Why use an interface? It makes your code easier to test and swap out later.
    // You register the interface with DI, and ASP.NET Core automatically gives you
    // the real implementation wherever you ask for it.
    public interface IAuthService
    {
        // Takes a plain-text password, returns a bcrypt hash
        string HashPassword(string password);

        // Takes a plain-text password and a stored hash, returns true if they match
        bool VerifyPassword(string password, string hash);

        // Takes a User object, returns a signed JWT string like "eyJhbGci..."
        string GenerateToken(User user);
    }

    public class AuthService : IAuthService
    {
        // IConfiguration lets us read values from appsettings.json
        private readonly IConfiguration _configuration;

        // Constructor Injection: ASP.NET Core automatically provides IConfiguration
        // when it creates an AuthService instance (because we registered it in Program.cs)
        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashPassword(string password)
        {
            // BCrypt is a one-way hashing algorithm designed specifically for passwords.
            // WorkFactor 11 means it takes ~100ms to compute - slow enough to make
            // brute-force attacks impractical, but fast enough for normal use.
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        }

        public bool VerifyPassword(string password, string hash)
        {
            // BCrypt.Verify re-hashes the input password and compares it to the stored hash.
            // This is safe against timing attacks (unlike string comparison with ==).
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string GenerateToken(User user)
        {
            // Read JWT settings from appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiresInHours = int.Parse(jwtSettings["ExpiresInHours"]!);

            // Convert the secret string to bytes - cryptographic operations work on bytes
            var keyBytes = Encoding.UTF8.GetBytes(secret);

            // SymmetricSecurityKey wraps the bytes so JWT libraries can use it
            var signingKey = new SymmetricSecurityKey(keyBytes);

            // SigningCredentials packages the key + algorithm together
            // HmacSha256 is a standard, secure signing algorithm
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // Claims are KEY-VALUE pairs embedded inside the JWT token.
            // Anyone who has the token can READ these (they're base64-encoded, not encrypted).
            // But they CANNOT modify them without invalidating the signature.
            //
            // We store the user's ID and email in the token so we can identify them
            // on protected endpoints WITHOUT hitting the database on every request.
            var claims = new[]
            {
                // ClaimTypes.NameIdentifier = the user's unique ID
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // ClaimTypes.Email = the user's email
                new Claim(ClaimTypes.Email, user.Email),

                // ClaimTypes.Name = the user's display name
                new Claim(ClaimTypes.Name, user.Username),

                // JTI = JWT ID, a unique ID for this specific token (helps with revocation)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Build the complete JWT token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),    // Embed the claims
                Expires = DateTime.UtcNow.AddHours(expiresInHours), // Expiry time
                Issuer = issuer,                          // Who issued this token
                Audience = audience,                      // Who this token is for
                SigningCredentials = credentials          // How to sign it
            };

            // JwtSecurityTokenHandler is the factory that creates and serializes tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            // CreateToken builds the token object from the descriptor
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // WriteToken serializes it to the compact string format: "xxxxx.yyyyy.zzzzz"
            // This is what you copy and paste into the Swagger Authorize button
            return tokenHandler.WriteToken(token);
        }
    }
}
