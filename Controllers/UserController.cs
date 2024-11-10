using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using TaskManagementApp.Data;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var passwordValidationResult = ValidatePassword(model.Password);
            if (!passwordValidationResult.IsValid)
            {
                return BadRequest(new { message = passwordValidationResult.ErrorMessage });
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token = GenerateJwtToken(user);
            // Set the token in the cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUser model)
        {
            System.Diagnostics.Debug.WriteLine("Logging user in...");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return BadRequest(new { message = "Invalid email or password" });
            }
            var token = GenerateJwtToken(user);
            // Set the token in the cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = false, // Set HTTP-Only to false 
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Ok(new { token });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the token from the cookie
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("verify")]
        public IActionResult Verify()
        {
            var token = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                return Ok(new { message = "Token is valid" });
            }
            catch
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
        }

        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = HttpContext.Items["UserId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Unauthorized with userId empty" });
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var user = await _context.Users.FindAsync(userIdInt);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { user.Username });
        }

        [HttpGet("usernames")]
        public async Task<IActionResult> GetAllUsernames()
        {
            var usernames = await _context.Users
                .Select(u => u.Username)
                .ToListAsync();

            return Ok(usernames);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "Password must contain at least one digit");
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                return (false, "Password must contain at least one special character");
            }

            return (true, string.Empty);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }


    }
}