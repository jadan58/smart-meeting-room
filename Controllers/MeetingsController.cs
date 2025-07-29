using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IMapper _mapper;

        public MeetingController(IMeetingRepository meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingResponseDto>>> GetAllMeetings()
        {
            var meetings = await _meetingRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<MeetingResponseDto>>(meetings));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeetingResponseDto>> GetMeetingById(Guid id)
        {
            var meeting = await _meetingRepository.GetByIdAsync(id);
            if (meeting == null) return NotFound();
            return Ok(_mapper.Map<MeetingResponseDto>(meeting));
        }

        [HttpPost]
        public async Task<ActionResult<MeetingResponseDto>> CreateMeeting(CreateMeetingRequestDto dto)
        {
            var meeting = _mapper.Map<Meeting>(dto);
            meeting.Id = Guid.NewGuid();
            var created = await _meetingRepository.AddAsync(meeting);
            var response = _mapper.Map<MeetingResponseDto>(created);
            return CreatedAtAction(nameof(GetMeetingById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MeetingResponseDto>> UpdateMeeting(Guid id, UpdateMeetingRequestDto dto)
        {
            var meeting = _mapper.Map<Meeting>(dto);
            var updated = await _meetingRepository.UpdateAsync(id, meeting);
            if (updated == null) return NotFound();
            return Ok(_mapper.Map<MeetingResponseDto>(updated));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<MeetingResponseDto>> DeleteMeeting(Guid id)
        {
            var deleted = await _meetingRepository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<MeetingResponseDto>(deleted));
        }

        // Notes
        [HttpPost("{meetingId}/notes")]
        public async Task<ActionResult<NoteDto>> AddNote(Guid meetingId, CreateNoteRequestDto dto)
        {
            var note = _mapper.Map<Note>(dto);
            note.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddNoteAsync(meetingId, note);
            if (added == null) return NotFound();
            return Ok(_mapper.Map<NoteDto>(added));
        }

        [HttpPut("{meetingId}/notes/{noteId}")]
        public async Task<ActionResult<NoteDto>> UpdateNote(Guid meetingId, Guid noteId, UpdateNoteRequestDto dto)
        {
            var note = _mapper.Map<Note>(dto);
            var updated = await _meetingRepository.UpdateNoteAsync(meetingId, noteId, note);
            if (updated == null) return NotFound();
            return Ok(_mapper.Map<NoteDto>(updated));
        }

        [HttpDelete("{meetingId}/notes/{noteId}")]
        public async Task<ActionResult<NoteDto>> DeleteNote(Guid meetingId, Guid noteId)
        {
            var deleted = await _meetingRepository.DeleteNoteAsync(meetingId, noteId);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<NoteDto>(deleted));
        }

        // Action Items
        [HttpPost("{meetingId}/action-items")]
        public async Task<ActionResult<ActionItemDto>> AddActionItem(Guid meetingId, CreateActionItemRequestDto dto)
        {
            var item = _mapper.Map<ActionItem>(dto);
            item.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddActionItemAsync(meetingId, item);
            if (added == null) return NotFound();
            return Ok(_mapper.Map<ActionItemDto>(added));
        }

        [HttpPut("{meetingId}/action-items/{itemId}")]
        public async Task<ActionResult<ActionItemDto>> UpdateActionItem(Guid meetingId, Guid itemId, UpdateActionItemRequestDto dto)
        {
            var item = _mapper.Map<ActionItem>(dto);
            var updated = await _meetingRepository.UpdateActionItemAsync(meetingId, itemId, item);
            if (updated == null) return NotFound();
            return Ok(_mapper.Map<ActionItemDto>(updated));
        }

        [HttpDelete("{meetingId}/action-items/{itemId}")]
        public async Task<ActionResult<ActionItemDto>> DeleteActionItem(Guid meetingId, Guid itemId)
        {
            var deleted = await _meetingRepository.DeleteActionItemAsync(meetingId, itemId);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<ActionItemDto>(deleted));
        }

        // Invitees
        [HttpPost("{meetingId}/invitees")]
        public async Task<ActionResult<InviteeDto>> AddInvitee(Guid meetingId, CreateInviteeRequestDto dto)
        {
            var invitee = _mapper.Map<Invitee>(dto);
            invitee.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddInviteeAsync(meetingId, invitee);
            if (added == null) return NotFound();
            return Ok(_mapper.Map<InviteeDto>(added));
        }

        [HttpDelete("{meetingId}/invitees/{inviteeId}")]
        public async Task<ActionResult<InviteeDto>> DeleteInvitee(Guid meetingId, Guid inviteeId)
        {
            var deleted = await _meetingRepository.DeleteInviteeAsync(meetingId, inviteeId);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<InviteeDto>(deleted));
        }

        // Attachments
        [HttpPost("{meetingId}/attachments")]
        public async Task<ActionResult<AttachmentDto>> AddAttachment(Guid meetingId, CreateAttachmentRequestDto dto)
        {
            var attachment = _mapper.Map<Attachment>(dto);
            attachment.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddAttachmentAsync(meetingId, attachment);
            if (added == null) return NotFound();
            return Ok(_mapper.Map<AttachmentDto>(added));
        }

        [HttpDelete("{meetingId}/attachments/{attachmentId}")]
        public async Task<ActionResult<AttachmentDto>> DeleteAttachment(Guid meetingId, Guid attachmentId)
        {
            var deleted = await _meetingRepository.DeleteAttachmentAsync(meetingId, attachmentId);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<AttachmentDto>(deleted));
        }
    }
}