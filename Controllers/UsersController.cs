using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;
using System.Security.Claims;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserRepository userRepository, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();

            var userDtos = new List<ApplicationUserDto>();
            foreach (var user in users)
            {
                var dto = _mapper.Map<ApplicationUserDto>(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(dto);
            }

            return Ok(userDtos);
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return NotFound();
            Console.WriteLine(user);
            var dto = _mapper.Map<UserReponseDTO>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Ok(dto);
        }


        [HttpGet("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserReponseDTO>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return Ok(dto);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return NotFound();
            var dto = _mapper.Map<UserReponseDTO>(user);
            dto.Roles = dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Ok(dto);
        }
        [HttpGet("count")]
        public async Task<IActionResult> GetUsersCount()
        {
            var users = await _userRepository.GetAllAsync();
            var count = users.Count();
            return Ok(new { Count = count });
        }
    }
}
