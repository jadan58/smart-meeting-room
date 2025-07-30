using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RoomController(IRoomRepository roomRepository, IMapper mapper, IFeatureRepository featureRepository)
        {
            _roomRepository = roomRepository;
            _mapper = mapper;
            _featureRepository = featureRepository;
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
            var existingRoom = await _roomRepository.GetByIdAsync(id);
            if (existingRoom == null)
                return NotFound();

            // Validate feature IDs
            var validFeatures = await _featureRepository.GetByIdsAsync(updateDto.FeatureIds);
            if (validFeatures.Count != updateDto.FeatureIds.Count)
                return BadRequest("One or more feature IDs are invalid.");

            // Map update DTO fields into existing room (excluding RoomFeatures)
            _mapper.Map(updateDto, existingRoom);

            // Update RoomFeatures:
            // Clear existing and repopulate with new features
            existingRoom.RoomFeatures.Clear();
            existingRoom.RoomFeatures = validFeatures.Select(f => new RoomFeature
            {
                Id = Guid.NewGuid(),
                RoomId = existingRoom.Id,
                FeatureId = f.Id,
                Feature = f  // optional
            }).ToList();

            var updatedRoom = await _roomRepository.UpdateAsync(id, existingRoom);

            var responseDto = _mapper.Map<RoomResponseDto>(updatedRoom);
            return Ok(responseDto);
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
