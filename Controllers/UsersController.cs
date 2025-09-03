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
using System.ComponentModel;
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
        private readonly AppDbContext _dbContext;

        public UsersController(
            IUserRepository userRepository,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            AppDbContext dbContext)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _dbContext = dbContext;
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
                .Where(m => m.EndTime > DateTime.UtcNow)
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
                .Where(inv => inv.UserId == guidUserId && inv.Status == "Pending" && inv.Meeting.EndTime > DateTime.UtcNow)
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
        [HttpGet("me/meetings/all")]
        [Authorize]
        public async Task<IActionResult> GetAllMeetings([FromServices] AppDbContext dbContext)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            // Get organized meetings
            var organizedMeetings = await dbContext.Meetings
                .Where(m => m.UserId == guidUserId)
                .Select(m => new { Meeting = m, Type = "organized" })
                .ToListAsync();

            // Get accepted invited meetings
            var invitedMeetings = await dbContext.Invitees
                .Where(inv => inv.UserId == guidUserId
                             && inv.Status == "Answered"
                             && inv.Attendance == "Accepted"
                             && inv.Meeting.UserId != guidUserId) // Exclude meetings already included in organized
                .Select(inv => new { Meeting = inv.Meeting, Type = "invited" })
                .ToListAsync();

            // Combine and sort by start time (most recent first)
            var allMeetings = organizedMeetings.Concat(invitedMeetings)
                .OrderByDescending(x => x.Meeting.StartTime)
                .Select(x => new
                {
                    Meeting = _mapper.Map<AllMeetingsDto>(x.Meeting),
                    Type = x.Type
                })
                .ToList();

            return Ok(allMeetings);
        }
        [HttpGet("me/invites/accepted")]
        [Authorize]
        public async Task<IActionResult> GetAcceptedInvites([FromServices] AppDbContext dbContext, int page = 1, int pageSize = 3)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            var acceptedInvites = await dbContext.Invitees
                .Where(inv => inv.UserId == guidUserId && inv.Status == "Answered" && inv.Attendance == "Accepted" && inv.Meeting.EndTime > DateTime.UtcNow)
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

            [HttpGet("me/meetings/previous/all")]
            [Authorize]
            public async Task<IActionResult> GetAllPreviousMeetings([FromServices] AppDbContext dbContext,int page=1,int pageSize =3)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();

                var guidUserId = Guid.Parse(userId);

                // Get organized meetings
                var organizedMeetings = await dbContext.Meetings
                    .Where(m => m.UserId == guidUserId && m.EndTime < DateTime.UtcNow)
                    .Select(m => new { Meeting = m, Type = "Organized" })
                    .ToListAsync();

                // Get accepted invited meetings
                var invitedMeetings = await dbContext.Invitees
                    .Where(inv => inv.UserId == guidUserId
                                 && inv.Status == "Answered"
                                 && inv.Attendance == "Accepted"
                                 && inv.Meeting.EndTime < DateTime.UtcNow
                                 && inv.Meeting.UserId != guidUserId) // Exclude meetings already included in organized
                    .Select(inv => new { Meeting = inv.Meeting, Type = "Invited" })
                    .ToListAsync();

                // Combine and sort by start time (most recent first)
                var allPreviousMeetings = organizedMeetings.Concat(invitedMeetings)
                    .OrderByDescending(x => x.Meeting.StartTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        Meeting = _mapper.Map<AllMeetingsDto>(x.Meeting),
                        Type = x.Type
                    })
                    .ToList();

                return Ok(allPreviousMeetings);
            }

        [HttpGet("me/meetings/dailycount")]
        [Authorize]
        public async Task<IActionResult> GetDailyMeetingCount([FromServices] AppDbContext dbContext)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var guidUserId = Guid.Parse(userId);

            // Get today's start and end in UTC
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Count meetings organized by the user today
            var dailyMeetingCount = await dbContext.Meetings
                .Where(m => m.UserId == guidUserId
                            && m.StartTime >= today
                            && m.StartTime < tomorrow)
                .CountAsync();

            return Ok(new { Count = dailyMeetingCount });
        }

        [HttpGet("me/meetings/heatmap")]
        [Authorize]
        public async Task<IActionResult> GetUserMeetingsHeatmap()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var today = DateTime.UtcNow.Date;
            var oneYearAgo = today.AddYears(-1);

            // Query meetings organized or accepted by the user in the past year
            var meetingsPerDay = await _dbContext.Meetings
                .Where(m => m.StartTime >= oneYearAgo &&
                           (m.UserId == userId ||
                            m.Invitees.Any(i => i.UserId == userId && i.Status == "Answered" && i.Attendance == "Accepted")))
                .GroupBy(m => m.StartTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Fill missing dates with 0
            var meetingCounts = new Dictionary<string, int>();
            for (var date = oneYearAgo; date <= today; date = date.AddDays(1))
            {
                var match = meetingsPerDay.FirstOrDefault(m => m.Date == date);
                meetingCounts[date.ToString("yyyy-MM-dd")] = match?.Count ?? 0;
            }

            return Ok(meetingCounts);
        }
        [HttpPost("me/upload-profile")]
        [Authorize]
        public async Task<IActionResult> UploadRoomImage(IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
                return NotFound("User not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type. Allowed: .jpg, .jpeg, .png");

            var uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "users");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Delete old image if exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldImagePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), user.ProfilePictureUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            var relativePath = $"/uploads/users/{fileName}";
            await _userRepository.UpdateImageAsync(Guid.Parse(userId), relativePath);

            return Ok(new { userId, ImageUrl = relativePath });
        }
    }
}