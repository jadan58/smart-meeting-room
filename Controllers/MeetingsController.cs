using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;
using System.Security.Claims;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly IRoomRepository roomRepository;
        private readonly IMeetingRepository _meetingRepository;
        private readonly IRecurringBookingRepository _recurringBookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MeetingController> logger;

        public MeetingController(IRoomRepository roomRepository,
            IMeetingRepository meetingRepository,
            IRecurringBookingRepository recurringBookingRepository,
            IMapper mapper, ILogger<MeetingController> logger)
        {
            this.roomRepository = roomRepository;
            _meetingRepository = meetingRepository;
            _recurringBookingRepository = recurringBookingRepository;
            _mapper = mapper;
            this.logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MeetingResponseDto>>> GetAllMeetings()
        {
            var meetings = await _meetingRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<AllMeetingsDto>>(meetings));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<MeetingResponseDto>> GetMeetingById(Guid id)
        {
            var userId = GetUserId();
            var meeting = await _meetingRepository.GetByIdAsync(id);
            if (meeting == null) return NotFound();
            if (!IsCreatorOrInvitee(meeting, userId))
                return Forbid("Only meeting creator or invitees can view meeting.");
            return Ok(_mapper.Map<MeetingResponseDto>(meeting));
        }


        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MeetingResponseDto>> CreateMeeting(CreateMeetingRequestDto dto)
        {
            var userId = GetUserId();
            var meeting = _mapper.Map<Meeting>(dto);
            meeting.Id = Guid.NewGuid();
            meeting.UserId = userId;

            if (meeting.StartTime >= meeting.EndTime)
                return BadRequest("StartTime must be before EndTime.");

            if (await IsRoomBookedAsync(meeting.RoomId,meeting.Id, meeting.StartTime, meeting.EndTime))
                return BadRequest("The room is already booked for the specified time.");

            var created = await _meetingRepository.AddAsync(meeting);
            return CreatedAtAction(nameof(GetMeetingById), new { id = created.Id }, _mapper.Map<MeetingResponseDto>(created));
        }

        [HttpPost("recurring")]
        [Authorize]
        public async Task<ActionResult<RecurringBookingResponseDto>> CreateRecurringMeeting(CreateRecurringMeetingRequestDTO dto)
        {
            var userId = GetUserId();

            if (dto.StartTime >= dto.EndTime)
                return BadRequest("StartTime must be before EndTime.");

            if (dto.RecurrenceEndDate < dto.StartTime)
                return BadRequest("RecurrenceEndDate must be on or after StartTime.");
            Guid Dummy = Guid.Empty;
            // Check initial booking conflict
            if (await IsRoomBookedAsync(dto.RoomId,Dummy,dto.StartTime, dto.EndTime))
                return BadRequest("The room is already booked for the specified time.");

            var recurringBookingId = Guid.NewGuid();
            var recBooking = _mapper.Map<RecurringBooking>(dto);
            recBooking.Id = recurringBookingId;
            recBooking.UserId = userId;

            var addedRec = await _recurringBookingRepository.createAsync(recBooking);
            if (addedRec == null)
                return BadRequest("Failed to create recurring booking.");

            DateTime currentStart = dto.StartTime;
            DateTime currentEnd = dto.EndTime;
            TimeSpan interval = dto.RecurrencePattern switch
            {
                "Daily" => TimeSpan.FromDays(1),
                "Weekly" => TimeSpan.FromDays(7),
                "Monthly" => TimeSpan.FromDays(30),
                _ => throw new ArgumentException("Unsupported recurrence pattern")
            };

            var meetings = new List<Meeting>();
            while (currentStart <= dto.RecurrenceEndDate)
            {
                // Skip conflicts beyond the first if necessary, or stop
                if (!await IsRoomBookedAsync(dto.RoomId,Dummy, currentStart, currentEnd))
                {
                    meetings.Add(new Meeting
                    {
                        Id = Guid.NewGuid(),
                        RoomId = dto.RoomId,
                        UserId = userId,
                        RecurringBookingId = recurringBookingId,
                        Title = dto.Title,
                        Agenda = dto.Agenda,
                        StartTime = currentStart,
                        EndTime = currentEnd,
                        Status = "Scheduled",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                currentStart = currentStart.Add(interval);
                currentEnd = currentEnd.Add(interval);
            }

            await _meetingRepository.AddRangeAsync(meetings);

            return Ok(new RecurringBookingResponseDto
            {
                Id = recurringBookingId,
                Title = dto.Title,
                TotalMeetings = meetings.Count,
                Pattern = dto.RecurrencePattern,
                RecurrenceEndDate = dto.RecurrenceEndDate
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<MeetingResponseDto>> UpdateMeeting(Guid id, UpdateMeetingRequestDto dto)
        {
            var userId = GetUserId();
            var existing = await _meetingRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (!IsCreator(existing, userId))
                return Forbid("Only meeting creator can update the meeting.");

            var updatedModel = _mapper.Map<Meeting>(dto);
            if (await IsRoomBookedAsync(updatedModel.RoomId,id, updatedModel.StartTime, updatedModel.EndTime))
                return BadRequest("The room is already booked for the specified time.");

            var updated = await _meetingRepository.UpdateAsync(id, updatedModel);
            return Ok(_mapper.Map<MeetingResponseDto>(updated));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<MeetingResponseDto>> DeleteMeeting(Guid id)
        {
            var userId = GetUserId();
            var meeting = await _meetingRepository.GetByIdAsync(id);
            if (meeting == null) return NotFound();
            if (!IsCreator(meeting, userId))
                return Forbid("Only meeting creator can delete the meeting.");

            var deleted = await _meetingRepository.DeleteAsync(id);
            return Ok(_mapper.Map<MeetingResponseDto>(deleted));
        }

        [HttpPost("{meetingId}/notes")]
        [Authorize]
        public async Task<ActionResult<NoteDto>> AddNote(Guid meetingId, CreateNoteRequestDto dto)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            if (!IsCreatorOrInvitee(meeting, userId))
                return Forbid("Only meeting creator or invitees can add notes.");

            var note = _mapper.Map<Note>(dto);
            note.Id = Guid.NewGuid();
            note.CreatedByUserId = userId;
            var added = await _meetingRepository.AddNoteAsync(meetingId, note);
            return Ok(_mapper.Map<NoteDto>(added));
        }

        [HttpPut("{meetingId}/notes/{noteId}")]
        [Authorize] 
        public async Task<ActionResult<NoteDto>> UpdateNote(Guid meetingId, Guid noteId, UpdateNoteRequestDto dto)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var note = meeting.Notes.FirstOrDefault(n => n.Id == noteId);
            if (note == null) return NotFound();
            if (!IsCreator(meeting, userId) && !IsNoteOwner(note, userId))
                return Forbid("Only meeting creator or note owner can update this note.");

            var updatedNote = _mapper.Map<Note>(dto);
            var updated = await _meetingRepository.UpdateNoteAsync(meetingId, noteId, updatedNote);
            return Ok(_mapper.Map<NoteDto>(updated));
        }

        [HttpDelete("{meetingId}/notes/{noteId}")]
        [Authorize]
        public async Task<ActionResult<NoteDto>> DeleteNote(Guid meetingId, Guid noteId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var note = meeting.Notes.FirstOrDefault(n => n.Id == noteId);
            if (note == null) return NotFound();
            if (!IsCreator(meeting, userId) && !IsNoteOwner(note, userId))
                return Forbid("Only meeting creator or note owner can delete this note.");

            var deleted = await _meetingRepository.DeleteNoteAsync(meetingId, noteId);
            return Ok(_mapper.Map<NoteDto>(deleted));
        }

        [HttpPost("{meetingId}/action-items")]
        [Authorize]
        public async Task<ActionResult<ActionItemDto>> AddActionItem(Guid meetingId, CreateActionItemRequestDto dto)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            if (!IsCreator(meeting, userId))
                return Forbid("Only meeting creator can add action items.");
            if (!IsCreatorOrInvitee(meeting, dto.AssignedToUserId))
                return Forbid("Action items can only be assigned to invitees");
            var item = _mapper.Map<ActionItem>(dto);
            item.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddActionItemAsync(meetingId, item);
            return Ok(_mapper.Map<ActionItemDto>(added));
        }

        [HttpPut("{meetingId}/action-items/{itemId}/toggle-status")]
        [Authorize]
        public async Task<ActionResult<ActionItemDto>> ToggleStatus(Guid meetingId, Guid itemId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var item = meeting.ActionItems.FirstOrDefault(ai => ai.Id == itemId);
            if (item == null) return NotFound();
            if (userId!=item.AssignedToUserId)
                return Forbid("Only the user that this task is assigned to can toggle its status.");
            item.Status = item.Status == "Pending" ? "Submitted" : "Pending";
            var updated = await _meetingRepository.UpdateActionItemAsync(meetingId, itemId, item);
            return Ok(_mapper.Map<ActionItemDto>(updated));
        }
        [HttpPut("{meetingId}/action-items/{itemId}/toggle-judgment")]
        [Authorize]
        public async Task<ActionResult<ActionItemDto>> ToggleJudgment(Guid meetingId, Guid itemId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var item = meeting.ActionItems.FirstOrDefault(ai => ai.Id == itemId);
            if (item == null) return NotFound();
            if(item.Status!="Submitted")
                return BadRequest("Only submitted tasks can be judged.");
            if (!IsCreator(meeting, userId))
                return Forbid("Only the creator of this task can toggle its judgment.");
            item.Judgment = item.Judgment == "Rejected" ? "Accepted" : "Rejected";
            var updated = await _meetingRepository.UpdateActionItemAsync(meetingId, itemId, item);
            return Ok(_mapper.Map<ActionItemDto>(updated));
        }
        [HttpDelete("{meetingId}/action-items/{itemId}")]
        [Authorize]
        public async Task<ActionResult<ActionItemDto>> DeleteActionItem(Guid meetingId, Guid itemId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            if (!IsCreator(meeting, userId))
                return Forbid("Only meeting creator can delete action items.");

            var deleted = await _meetingRepository.DeleteActionItemAsync(meetingId, itemId);
            return Ok(_mapper.Map<ActionItemDto>(deleted));
        }

        [HttpPost("{meetingId}/invitees")]
        [Authorize]
        public async Task<ActionResult<InviteeDto>> AddInvitee(Guid meetingId, CreateInviteeRequestDto dto)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            if (!IsCreator(meeting, userId)) return Forbid("Only meeting creator can add invitees.");
            if (IsRoomFull(meeting.Room, meeting)) return Forbid("Room is full, cannot add more invitees.");

            var invitee = _mapper.Map<Invitee>(dto);
            invitee.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddInviteeAsync(meetingId, invitee);
            return Ok(_mapper.Map<InviteeDto>(added));
        }

        [HttpDelete("{meetingId}/invitees/{inviteeId}")]
        [Authorize]
        public async Task<ActionResult<InviteeDto>> DeleteInvitee(Guid meetingId, Guid inviteeId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            if (!IsCreator(meeting, userId)) return Forbid("Only meeting creator can delete invitees.");
            if (!IsCreatorOrInvitee(meeting, inviteeId)) return BadRequest("User is not invited.");
            var deleted = await _meetingRepository.DeleteInviteeAsync(meetingId, inviteeId);
            return Ok(_mapper.Map<InviteeDto>(deleted));
        }

        [HttpPost("{meetingId}/attachments")]
        [Authorize]
        public async Task<ActionResult<AttachmentDto>> AddAttachment(Guid meetingId, CreateAttachmentRequestDto dto)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            if (!IsCreatorOrInvitee(meeting, userId)) return Forbid("Only meeting creator or invitees can add attachments.");

            var attachment = _mapper.Map<Attachment>(dto);
            attachment.Id = Guid.NewGuid();
            attachment.UploadedByUserId = userId;
            attachment.MeetingId = meetingId;

            var added = await _meetingRepository.AddAttachmentAsync(meetingId, attachment);
            return Ok(_mapper.Map<AttachmentDto>(added));
        }

        [HttpDelete("{meetingId}/attachments/{attachmentId}")]
        [Authorize]
        public async Task<ActionResult<AttachmentDto>> DeleteAttachment(Guid meetingId, Guid attachmentId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var attachment = meeting.Attachments.FirstOrDefault(a => a.Id == attachmentId);
            if (attachment == null) return NotFound();
            if (!IsAttachmentUploader(attachment, userId) && !IsCreator(meeting, userId))
                return Forbid("Only attachment uploader or meeting creator can delete this attachment.");

            var deleted = await _meetingRepository.DeleteAttachmentAsync(meetingId, attachmentId);
            return Ok(_mapper.Map<AttachmentDto>(deleted));
        }

        // PRIVATE HELPERS
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private bool IsCreator(Meeting meeting, Guid userId) => meeting.UserId == userId;
        private bool IsCreatorOrInvitee(Meeting meeting, Guid userId) => IsCreator(meeting, userId) || meeting.Invitees.Any(i => i.UserId == userId);
        private bool IsNoteOwner(Note note, Guid userId) => note.CreatedByUserId == userId;
        private bool IsAttachmentUploader(Attachment attachment, Guid userId) => attachment.UploadedByUserId == userId;
        private bool IsRoomFull(Room room, Meeting meeting)
        {
            logger.LogError(room.Capacity + " " + meeting.Invitees.Count());
            return room == null || meeting.Invitees.Count() >= room.Capacity;
        }
        private async Task<bool> IsRoomBookedAsync(Guid roomId,Guid meetingId, DateTime startTime, DateTime endTime)
        {
            var meetings = await _meetingRepository.GetAllAsync();
            return meetings.Any(m => m.RoomId == roomId && m.StartTime < endTime && m.EndTime > startTime && m.Status != "Cancelled"&&m.Id!=meetingId);
        }
        private async Task<Meeting> EnsureMeetingAsync(Guid meetingId)
        {
            return await _meetingRepository.GetByIdAsync(meetingId);
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetMeetingsCount()
        {
            var meetings = await _meetingRepository.GetAllAsync();
            return Ok(meetings.Count);
        }

        [HttpGet("top-rooms")]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetTopBookedRooms()
        {
            var meetings = await _meetingRepository.GetAllAsync();

            var topRoomIds = meetings
                .Where(m => m.Status != "Cancelled")
                .GroupBy(m => m.RoomId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToList();

            var rooms = await roomRepository.GetAllAsync(); 
            var topRooms = rooms
                .Where(r => topRoomIds.Contains(r.Id))
                .OrderBy(r => topRoomIds.IndexOf(r.Id)) 
                .Take(3)
                .ToList();

            var result = _mapper.Map<IEnumerable<RoomResponseDto>>(topRooms);
            return Ok(result);
        }

    }
}
