using AutoMapper;
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

        public RoomController(IRoomRepository roomRepository, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _mapper = mapper;
        }

        // GET: api/Room
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAllRooms()
        {
            var rooms = await _roomRepository.GetAllAsync();
            var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);
            return Ok(roomDtos);
        }

        // GET: api/Room/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetRoomById(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return NotFound();

            var roomDto = _mapper.Map<RoomResponseDto>(room);
            return Ok(roomDto);
        }

        // POST: api/Room
        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> CreateRoom(CreateRoomRequestDto createDto)
        {
            var room = _mapper.Map<Room>(createDto);
            room.Id = Guid.NewGuid();

            var createdRoom = await _roomRepository.AddAsync(room);
            var roomDto = _mapper.Map<RoomResponseDto>(createdRoom);

            return CreatedAtAction(nameof(GetRoomById), new { id = roomDto.Id }, roomDto);
        }

        // PUT: api/Room/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<RoomResponseDto>> UpdateRoom(Guid id, UpdateRoomRequestDto updateDto)
        {
            var updatedRoom = _mapper.Map<Room>(updateDto);
            var result = await _roomRepository.UpdateAsync(id, updatedRoom);

            if (result == null)
                return NotFound();

            var resultDto = _mapper.Map<RoomResponseDto>(result);
            return Ok(resultDto);
        }

        // DELETE: api/Room/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<RoomResponseDto>> DeleteRoom(Guid id)
        {
            var deletedRoom = await _roomRepository.DeleteAsync(id);
            if (deletedRoom == null)
                return NotFound();

            var roomDto = _mapper.Map<RoomResponseDto>(deletedRoom);
            return Ok(roomDto);
        }
    }
}
