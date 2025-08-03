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
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAllRooms()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<RoomResponseDto>>(rooms));
        }

        [HttpGet("{id}")]
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
    }
}
