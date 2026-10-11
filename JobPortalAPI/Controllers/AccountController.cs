
using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JobPortalDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AccountController(
            JobPortalDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FullName) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Name, email and password are required.");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("Email already exists.");

            user.Id = 0;
            user.Email = user.Email.Trim().ToLowerInvariant();

            var password = user.PasswordHash;
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.Role = "JobSeeker";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Trim())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = user.FullName,
                role = user.Role.Trim()
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
