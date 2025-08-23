using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IMapper _mapper;
        private readonly IFeatureRepository _featureRepository;
        private readonly AppDbContext _context;

        public RoomController(IRoomRepository roomRepository, IMapper mapper, IFeatureRepository featureRepository, AppDbContext _context)
        {
            _roomRepository = roomRepository;
            _mapper = mapper;
            _featureRepository = featureRepository;
            this._context = _context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAllRooms()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<RoomResponseDto>>(rooms));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<RoomResponseDto>> GetRoomById(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound();

            return Ok(_mapper.Map<RoomResponseDto>(room));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomResponseDto>> CreateRoom(CreateRoomRequestDto createDto)
        {
            // Validate that all provided feature IDs exist
            var validFeatures = await _featureRepository.GetByIdsAsync(createDto.FeatureIds);
            if (validFeatures.Count != createDto.FeatureIds.Count)
                return BadRequest("One or more feature IDs are invalid.");

            // Map DTO to domain model
            var room = _mapper.Map<Room>(createDto);
            room.Id = Guid.NewGuid();

            // Create RoomFeature join entities
            room.RoomFeatures = validFeatures.Select(f => new RoomFeature
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                FeatureId = f.Id,
                Feature = f  // optional: include Feature navigation for EF tracking
            }).ToList();

            var createdRoom = await _roomRepository.AddAsync(room);

            var responseDto = _mapper.Map<RoomResponseDto>(createdRoom);
            return CreatedAtAction(nameof(GetRoomById), new { id = createdRoom.Id }, responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomResponseDto>> UpdateRoom(Guid id, UpdateRoomRequestDto updateDto)
        {
            var room = _mapper.Map<Room>(updateDto);

            var updatedRoom = await _roomRepository.UpdateAsync(id, room);

            var responseDto = _mapper.Map<RoomResponseDto>(updatedRoom);
            return Ok(responseDto);
        }
        [HttpPost("{roomId}/features/{featureId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddFeatureToRoom(Guid roomId, Guid featureId)
        {
            // Check if Room and Feature exist
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId);
            var featureExists = await _context.Features.AnyAsync(f => f.Id == featureId);

            if (!roomExists || !featureExists)
                return NotFound("Room or Feature not found.");

            // Check if this RoomFeature already exists
            var exists = await _context.RoomFeatures.AnyAsync(rf =>
                rf.RoomId == roomId && rf.FeatureId == featureId);

            if (exists)
                return Conflict("Feature already added to this room.");

            // Create and add the new RoomFeature
            var roomFeature = new RoomFeature
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                FeatureId = featureId
            };

            _context.RoomFeatures.Add(roomFeature);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{roomId}/features/{featureId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFeatureFromRoom(Guid roomId, Guid featureId)
        {
            var roomFeature = await _context.RoomFeatures
                .FirstOrDefaultAsync(rf => rf.RoomId == roomId && rf.FeatureId == featureId);

            if (roomFeature == null)
                return NotFound("This feature is not assigned to the room.");

            _context.RoomFeatures.Remove(roomFeature);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoomResponseDto>> DeleteRoom(Guid id)
        {
            var deletedRoom = await _roomRepository.DeleteAsync(id);
            if (deletedRoom == null)
                return NotFound();

            return Ok(_mapper.Map<RoomResponseDto>(deletedRoom));
        }

        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadRoomImage(Guid id, IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound("Room not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type. Allowed: .jpg, .jpeg, .png, .gif");

            var uploadsFolder = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "rooms");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Delete old image if exists
            if (!string.IsNullOrEmpty(room.ImageUrl))
            {
                var oldImagePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), room.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            var relativePath = $"/uploads/rooms/{fileName}";
            await _roomRepository.UpdateImageAsync(id, relativePath);

            return Ok(new { room.Id, ImageUrl = relativePath });
        }

        [HttpDelete("{id}/delete-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoomImage(Guid id, [FromServices] IWebHostEnvironment env)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound("Room not found.");

            if (string.IsNullOrEmpty(room.ImageUrl))
                return BadRequest("This room has no image to delete.");

            var imagePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), room.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            await _roomRepository.UpdateImageAsync(id, null);

            return NoContent();
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetRoomsCount()
        {
            var count = await _context.Rooms.CountAsync();
            return Ok(count);
        }
    }
}
