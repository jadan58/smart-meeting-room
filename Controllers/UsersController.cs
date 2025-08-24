using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using SmartMeetingRoomAPI.Data;
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
        [HttpGet("me/meetings/organized")]
        [Authorize]
        public async Task<IActionResult> GetOrganizedMeetings(int page = 1, int pageSize = 3)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserMeetingResponseDto>(user);

            // Sort by start time
            dto.OrganizedMeetings = dto.OrganizedMeetings
                .OrderBy(m => m.StartTime) // assuming StartTime is DateTime
                .Where(m => m.StartTime > DateTime.UtcNow)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(dto.OrganizedMeetings);
        }
        [HttpGet("me/meetings/invited")]
        [Authorize]
        public async Task<IActionResult> GetInvitedMeetings(int page = 1, int pageSize = 3)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserMeetingResponseDto>(user);

            dto.InvitedMeetings = dto.InvitedMeetings
                .OrderBy(m => m.StartTime)
                .Where(m => m.StartTime > DateTime.UtcNow)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(dto.InvitedMeetings);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserReponseDTO>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return Ok(dto);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
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

        [HttpGet("me/invites/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingInvites([FromServices] AppDbContext dbContext, int page = 1, int pageSize = 3)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            var pendingInvites = await dbContext.Invitees
                .Where(inv => inv.UserId == guidUserId && inv.Status == "Pending" && inv.Meeting.StartTime > DateTime.UtcNow)
                .OrderBy(inv => inv.Meeting.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(inv => new MeetingWithInviteDto
                {
                    InviteId = inv.Id,
                    Meeting = _mapper.Map<AllMeetingsDto>(inv.Meeting)
                })
                .ToListAsync();

            return Ok(pendingInvites);
        }
        [HttpGet("me/invites/accepted")]
        [Authorize]
        public async Task<IActionResult> GetAcceptedInvites([FromServices] AppDbContext dbContext, int page = 1, int pageSize = 3)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            var acceptedInvites = await dbContext.Invitees
                .Where(inv => inv.UserId == guidUserId && inv.Status == "Answered" && inv.Attendance == "Accepted" && inv.Meeting.StartTime > DateTime.UtcNow)
                .OrderBy(inv => inv.Meeting.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(inv => new MeetingWithInviteDto
                {
                    InviteId = inv.Id,
                    Meeting = _mapper.Map<AllMeetingsDto>(inv.Meeting)
                })
                .ToListAsync();

            return Ok(acceptedInvites);
        }

    }
}
