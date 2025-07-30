using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartMeetingRoomAPI.DTOs.Auth;
using SmartMeetingRoomAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartMeetingRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
                return BadRequest("User already exists");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign Employee role automatically to all new registered users
            await _userManager.AddToRoleAsync(user, "Employee");

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized("Invalid credentials");

            var token = await GenerateJwtToken(user);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("firstName", user.FirstName ?? ""),
        new Claim("lastName", user.LastName ?? "")
    };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
