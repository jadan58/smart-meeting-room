using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;
using System.Security.Claims;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IMapper _mapper;

        public MeetingController(IMeetingRepository meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        // GET: api/Meeting (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MeetingResponseDto>>> GetAllMeetings()
        {
            var meetings = await _meetingRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<MeetingResponseDto>>(meetings));
        }

        // GET: api/Meeting/{id} (Creator only)
        [HttpGet("{id}")]
        public async Task<ActionResult<MeetingResponseDto>> GetMeetingById(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(id);
            if (meeting == null) return NotFound();
            bool isCreator = meeting.UserId == userId;
            bool isInvitee = meeting.Invitees.Any(i => i.UserId == userId);
            if (!isCreator && !isInvitee)
                return Forbid("Only meeting creator or invitees can view meeting.");
            return Ok(_mapper.Map<MeetingResponseDto>(meeting));
        }

        // POST: api/Meeting (Any authenticated user)
        [HttpPost]
        public async Task<ActionResult<MeetingResponseDto>> CreateMeeting(CreateMeetingRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = _mapper.Map<Meeting>(dto);
            meeting.Id = Guid.NewGuid();
            meeting.UserId = userId;
      
            if(await IsRoomBookedAsync(meeting.RoomId, meeting.StartTime, meeting.EndTime))
            {
                return BadRequest("The room is already booked for the specified time.");
            }
            var created = await _meetingRepository.AddAsync(meeting);
            var response = _mapper.Map<MeetingResponseDto>(created);
            return CreatedAtAction(nameof(GetMeetingById), new { id = response.Id }, response);
        }

        // PUT: api/Meeting/{id} (Creator only)
        [HttpPut("{id}")]
        public async Task<ActionResult<MeetingResponseDto>> UpdateMeeting(Guid id, UpdateMeetingRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existing = await _meetingRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.UserId != userId)
                return Forbid("Only meeting creator can update the meeting.");
            var updatedModel = _mapper.Map<Meeting>(dto);
            if (await IsRoomBookedAsync(updatedModel.RoomId, updatedModel.StartTime, updatedModel.EndTime))
            {
                return BadRequest("The room is already booked for the specified time.");
            }
            var updated = await _meetingRepository.UpdateAsync(id, updatedModel);
            return Ok(_mapper.Map<MeetingResponseDto>(updated));
        }

        // DELETE: api/Meeting/{id} (Creator only)
        [HttpDelete("{id}")]
        public async Task<ActionResult<MeetingResponseDto>> DeleteMeeting(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existing = await _meetingRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.UserId != userId)
                return Forbid("Only meeting creator can delete the meeting.");
            var deleted = await _meetingRepository.DeleteAsync(id);
            return Ok(_mapper.Map<MeetingResponseDto>(deleted));
        }

        // POST: api/Meeting/{meetingId}/notes (Creator or invitee)
        [HttpPost("{meetingId}/notes")]
        public async Task<ActionResult<NoteDto>> AddNote(Guid meetingId, CreateNoteRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            bool isCreator = meeting.UserId == userId;
            bool isInvitee = meeting.Invitees.Any(i => i.UserId == userId);
            if (!isCreator && !isInvitee)
                return Forbid("Only meeting creator or invitees can add notes.");
            var note = _mapper.Map<Note>(dto);
            note.Id = Guid.NewGuid();
            note.CreatedByUserId = userId;
            var added = await _meetingRepository.AddNoteAsync(meetingId, note);
            return Ok(_mapper.Map<NoteDto>(added));
        }

        // PUT: api/Meeting/{meetingId}/notes/{noteId} (Creator or note owner)
        [HttpPut("{meetingId}/notes/{noteId}")]
        public async Task<ActionResult<NoteDto>> UpdateNote(Guid meetingId, Guid noteId, UpdateNoteRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            var note = meeting.Notes.FirstOrDefault(n => n.Id == noteId);
            if (note == null) return NotFound();
            if (meeting.UserId != userId && note.CreatedByUserId != userId)
                return Forbid("Only meeting creator or note owner can update this note.");
            var updatedNote = _mapper.Map<Note>(dto);
            var updated = await _meetingRepository.UpdateNoteAsync(meetingId, noteId, updatedNote);
            return Ok(_mapper.Map<NoteDto>(updated));
        }

        // DELETE: api/Meeting/{meetingId}/notes/{noteId} (Creator or note owner)
        [HttpDelete("{meetingId}/notes/{noteId}")]
        public async Task<ActionResult<NoteDto>> DeleteNote(Guid meetingId, Guid noteId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            var note = meeting.Notes.FirstOrDefault(n => n.Id == noteId);
            if (note == null) return NotFound();
            if (meeting.UserId != userId && note.CreatedByUserId != userId)
                return Forbid("Only meeting creator or note owner can delete this note.");
            var deleted = await _meetingRepository.DeleteNoteAsync(meetingId, noteId);
            return Ok(_mapper.Map<NoteDto>(deleted));
        }

        // POST: api/Meeting/{meetingId}/action-items (Creator only)
        [HttpPost("{meetingId}/action-items")]
        public async Task<ActionResult<ActionItemDto>> AddActionItem(Guid meetingId, CreateActionItemRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            if (meeting.UserId != userId)
                return Forbid("Only meeting creator can add action items.");
            var item = _mapper.Map<ActionItem>(dto);
            item.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddActionItemAsync(meetingId, item);
            return Ok(_mapper.Map<ActionItemDto>(added));
        }

        // PUT: api/Meeting/{meetingId}/action-items/{itemId} (Creator only)
        [HttpPut("{meetingId}/action-items/{itemId}")]
        public async Task<ActionResult<ActionItemDto>> UpdateActionItem(Guid meetingId, Guid itemId, UpdateActionItemRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            if (meeting.UserId != userId)
                return Forbid("Only meeting creator can update action items.");
            var updatedItem = _mapper.Map<ActionItem>(dto);
            var updated = await _meetingRepository.UpdateActionItemAsync(meetingId, itemId, updatedItem);
            return Ok(_mapper.Map<ActionItemDto>(updated));
        }

        // DELETE: api/Meeting/{meetingId}/action-items/{itemId} (Creator only)
        [HttpDelete("{meetingId}/action-items/{itemId}")]
        public async Task<ActionResult<ActionItemDto>> DeleteActionItem(Guid meetingId, Guid itemId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            if (meeting.UserId != userId)
                return Forbid("Only meeting creator can delete action items.");
            var deleted = await _meetingRepository.DeleteActionItemAsync(meetingId, itemId);
            return Ok(_mapper.Map<ActionItemDto>(deleted));
        }

        // POST: api/Meeting/{meetingId}/invitees (Creator only)
        [HttpPost("{meetingId}/invitees")]
        public async Task<ActionResult<InviteeDto>> AddInvitee(Guid meetingId, CreateInviteeRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);

            // Get the room for the meeting
            var room = meeting?.Room;

            // Check if the meeting exists
            if (meeting == null) return NotFound();

            //Check if the room is full
            if (IsRoomFull(room, meeting))
            {
                return Forbid("Room is full, cannot add more invitees.");
            }
            // Check if the user is the creator of the meeting
            if (meeting.UserId != userId)
                return Forbid("Only meeting creator can add invitees.");

            var invitee = _mapper.Map<Invitee>(dto);
            invitee.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddInviteeAsync(meetingId, invitee);
            return Ok(_mapper.Map<InviteeDto>(added));
        }

        // DELETE: api/Meeting/{meetingId}/invitees/{inviteeId} (Creator only)
        [HttpDelete("{meetingId}/invitees/{inviteeId}")]
        public async Task<ActionResult<InviteeDto>> DeleteInvitee(Guid meetingId, Guid inviteeId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            if (meeting.UserId != userId)
                return Forbid("Only meeting creator can delete invitees.");
            var deleted = await _meetingRepository.DeleteInviteeAsync(meetingId, inviteeId);
            return Ok(_mapper.Map<InviteeDto>(deleted));
        }

        // POST: api/Meeting/{meetingId}/attachments (Creator or invitee)
        [HttpPost("{meetingId}/attachments")]
        public async Task<ActionResult<AttachmentDto>> AddAttachment(Guid meetingId, CreateAttachmentRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            bool isCreator = meeting.UserId == userId;
            bool isInvitee = meeting.Invitees.Any(i => i.UserId == userId);
            if (!isCreator && !isInvitee)
                return Forbid("Only meeting creator or invitees can add attachments.");

            var attachment = _mapper.Map<Attachment>(dto);
            attachment.Id = Guid.NewGuid();
            attachment.UploadedByUserId = userId; // set uploader
            attachment.MeetingId = meetingId;      // ensure meeting relation

            var added = await _meetingRepository.AddAttachmentAsync(meetingId, attachment);
            return Ok(_mapper.Map<AttachmentDto>(added));
        }

        // DELETE: api/Meeting/{meetingId}/attachments/{attachmentId} (Uploader or creator)
        [HttpDelete("{meetingId}/attachments/{attachmentId}")]
        public async Task<ActionResult<AttachmentDto>> DeleteAttachment(Guid meetingId, Guid attachmentId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null) return NotFound();
            var attachment = meeting.Attachments.FirstOrDefault(a => a.Id == attachmentId);
            if (attachment == null) return NotFound();
            if (attachment.UploadedByUserId != userId && meeting.UserId != userId)
                return Forbid("Only attachment uploader or meeting creator can delete this attachment.");
            var deleted = await _meetingRepository.DeleteAttachmentAsync(meetingId, attachmentId);
            return Ok(_mapper.Map<AttachmentDto>(deleted));
        }
        private bool IsRoomFull(Room room, Meeting meeting)
        {
            if (room == null) return true;
            // Assuming room capacity is stored in the Room model
            int currentOccupancy = meeting.Invitees.Count();
            return currentOccupancy >= room.Capacity;
        }
        private async Task<bool> IsRoomBookedAsync(Guid roomId, DateTime startTime, DateTime endTime)
        {
            var meetings = await _meetingRepository.GetAllAsync();

            return meetings.Any(m =>
                m.RoomId == roomId &&
                m.StartTime < endTime &&
                m.EndTime > startTime &&
                m.Status != "Cancelled");
        }


    }
}
