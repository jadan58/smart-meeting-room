using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
                return BadRequest("User already exists");

            // Validate role
            var validRoles = new[] { "Employee", "Guest" };
            if (string.IsNullOrWhiteSpace(dto.Role) || !validRoles.Contains(dto.Role))
                return BadRequest("Invalid role. Valid roles are: Employee, Guest.");

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

            // Assign selected role
            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok("User registered successfully as " + dto.Role);
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

        [HttpPut("role/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(Guid id, UpdateUserRoleDTO updateUserDTO)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound("User not found");
            var validRoles = new[] { "Employee", "Guest", "Admin" };
            if (string.IsNullOrWhiteSpace(updateUserDTO.Role) || !validRoles.Contains(updateUserDTO.Role))
                return BadRequest("Invalid role. Valid roles are: Employee, Guest, Admin.");
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(updateUserDTO.Role))
                return BadRequest("User already has this role");
            // Remove all current roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest("Failed to remove current roles");
            // Add new role
            var addResult = await _userManager.AddToRoleAsync(user, updateUserDTO.Role);
            return addResult.Succeeded
                ? Ok($"User role updated to {updateUserDTO.Role}")
                : BadRequest("Failed to update user role");
        }

        [HttpPost("change-password")]
        [Authorize]

        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest("New password and confirmation do not match.");

            // Get the current user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password changed successfully.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User deleted successfully.");
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
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
