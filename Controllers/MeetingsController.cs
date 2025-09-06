using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.DTOs;
using SmartMeetingRoomAPI.Models;
using SmartMeetingRoomAPI.Repositories;
using System;
using System.Security.Claims;

namespace SmartMeetingRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IRoomRepository roomRepository;
        private readonly IMeetingRepository _meetingRepository;
        private readonly IRecurringBookingRepository _recurringBookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MeetingController> logger;
        private readonly IWebHostEnvironment _env;

        public MeetingController(AppDbContext dbContext,IRoomRepository roomRepository,
            IMeetingRepository meetingRepository,
            IRecurringBookingRepository recurringBookingRepository,
            IMapper mapper, ILogger<MeetingController> logger, IWebHostEnvironment env)
        {
            this.dbContext = dbContext;
            this.roomRepository = roomRepository;
            _meetingRepository = meetingRepository;
            _recurringBookingRepository = recurringBookingRepository;
            _mapper = mapper;
            this.logger = logger;
            _env = env;
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
                return StatusCode(403, "Access denied"); 
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

            if (await IsRoomBookedAsync(meeting.RoomId, meeting.Id, meeting.StartTime, meeting.EndTime))
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
            if (await IsRoomBookedAsync(dto.RoomId, Dummy, dto.StartTime, dto.EndTime))
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
                if (!await IsRoomBookedAsync(dto.RoomId, Dummy, currentStart, currentEnd))
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
                return StatusCode(403, "Access denied"); 

            var updatedModel = _mapper.Map<Meeting>(dto);
            if (await IsRoomBookedAsync(updatedModel.RoomId, id, updatedModel.StartTime, updatedModel.EndTime))
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
                return StatusCode(403, "Access denied"); 

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
                return StatusCode(403, "Access denied"); 

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
                return StatusCode(403, "Access denied"); 

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
                return StatusCode(403, "Access denied"); 

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
                return StatusCode(403, "Access denied"); 
            if (!IsCreatorOrInvitee(meeting, dto.AssignedToUserId))
                return StatusCode(403, "Access denied"); 
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
            if (userId != item.AssignedToUserId)
                return StatusCode(403, "Access denied"); 
            item.Status = item.Status == "Pending" ? "Submitted" : "Pending";
            var updated = await _meetingRepository.UpdateActionItemAsync(meetingId, itemId, item);
            return Ok(_mapper.Map<ActionItemDto>(updated));
        }
        [HttpPut("{meetingId}/action-items/{itemId}/accept")]
        [Authorize]
        public async Task<ActionResult<ActionItemDto>> Accept(Guid meetingId, Guid itemId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var item = meeting.ActionItems.FirstOrDefault(ai => ai.Id == itemId);
            if (item == null) return NotFound();
            if (item.Status != "Submitted")
                return BadRequest("Only submitted tasks can be judged.");
            if (!IsCreator(meeting, userId))
                return StatusCode(403, "Access denied"); 
            item.Judgment ="Accepted";
            var updated = await _meetingRepository.UpdateActionItemAsync(meetingId, itemId, item);
            return Ok(_mapper.Map<ActionItemDto>(updated));
        }
        [HttpPut("{meetingId}/action-items/{itemId}/reject")]
        [Authorize]
        public async Task<ActionResult<ActionItemDto>> Reject(Guid meetingId, Guid itemId)
        {
            var userId = GetUserId();
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            var item = meeting.ActionItems.FirstOrDefault(ai => ai.Id == itemId);
            if (item == null) return NotFound();
            if (item.Status != "Submitted")
                return BadRequest("Only submitted tasks can be judged.");
            if (!IsCreator(meeting, userId))
                return StatusCode(403, "Access denied"); 
            item.Judgment= "Rejected";
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
                return StatusCode(403, "Access denied"); 

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
            if (!IsCreator(meeting, userId)) return StatusCode(403, "Access denied"); 
            if (IsRoomFull(meeting.Room, meeting)) return BadRequest("Room is full, cannot add more invitees.");
            var duplicate = meeting.Invitees.Any(i => i.UserId == dto.UserId);
            if (duplicate) return BadRequest("User is already invited.");
            var invitee = _mapper.Map<Invitee>(dto);    
            invitee.Id = Guid.NewGuid();
            var added = await _meetingRepository.AddInviteeAsync(meetingId, invitee);
            return Ok(_mapper.Map<InviteeDto>(added));
        }

        [HttpDelete("{meetingId}/invitees/{inviteId}")]
        [Authorize]
        public async Task<ActionResult<InviteeDto>> DeleteInvitee(Guid inviteId)
        {
            var userId = GetUserId();
            var invite = dbContext.Invitees.Find(inviteId);
            if(invite == null)
                return NotFound("Invite not found.");
            var meeting = await EnsureMeetingAsync(invite.MeetingId);
            if (!IsCreator(meeting, userId)) return StatusCode(403, "Access denied"); 
            var deleted = await _meetingRepository.DeleteInviteeAsync(inviteId);
            return Ok(_mapper.Map<InviteeDto>(deleted));
        }

     

        // PRIVATE HELPERS
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
        [HttpPut("{meetingId}/invitees/{inviteId}/accept")]
        [Authorize]
        public async Task<ActionResult<InviteeDto>> AcceptInvite(Guid meetingId, Guid inviteId)
        {
            var userId = GetUserId();
            var invite = dbContext.Invitees.Find(inviteId);
            if(invite == null)
                return NotFound("Invite not found.");
          
            if (userId != invite.UserId)
                return StatusCode(403, "Access denied"); 

            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();

            invite.Status = "Answered";
            invite.Attendance = "Accepted";

            var updated = await _meetingRepository.UpdateInviteeAsync(inviteId,invite);
            return Ok(_mapper.Map<InviteeDto>(updated));
        }
        [HttpPut("{meetingId}/invitees/{inviteId}/decline")]
        [Authorize]
        public async Task<ActionResult<InviteeDto>> DeclineInvite(Guid meetingId, Guid inviteId)
        {
            var userId = GetUserId();
            var invite = dbContext.Invitees.Find(inviteId);
            if(invite == null)
                return NotFound("Invite not found.");
            if (userId != invite.UserId)
                return StatusCode(403, "Access denied"); 
            var meeting = await EnsureMeetingAsync(meetingId);
            if (meeting == null) return NotFound();
            invite.Status = "Answered";
            invite.Attendance = "Declined";
            var updated = await _meetingRepository.UpdateInviteeAsync(inviteId, invite);
            return Ok(_mapper.Map<InviteeDto>(updated));
        }

        [HttpPost("{meetingId}/action-items/{itemId}/assignment-attachments")]
        [Authorize]
        public async Task<IActionResult> UploadAssignmentAttachments(Guid itemId, List<IFormFile> files)
        {
            var item = await dbContext.ActionItems
                .Include(ai => ai.Meeting)
                .FirstOrDefaultAsync(ai => ai.Id == itemId);
            if (item == null)
                return NotFound("Action item not found.");
            if (!IsCreator(item.Meeting, GetUserId()))
                return StatusCode(403, "Access denied");
            if (files == null || !files.Any())
                return BadRequest("No files uploaded.");
            if (files.Count > 5)
                return BadRequest("You can upload a maximum of 5 files.");
            if (files.Any(f => f.Length > 5 * 1024 * 1024))
                return BadRequest("Each file must be less than 5MB.");
            if (files.Any(f => !new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".txt" }
                .Contains(Path.GetExtension(f.FileName).ToLower())))
                return BadRequest("Only .jpg, .jpeg, .png, .pdf, .docx, .xlsx, .txt files are allowed.");

            // Private folder outside wwwroot
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "upload", "action-items", itemId.ToString(), "assignment");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePaths = new List<string>();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Relative path for secure GET endpoint
                var relativePath = Path.Combine("action-items", itemId.ToString(), "assignment", fileName)
                                        .Replace("\\", "/");
                filePaths.Add(relativePath);
            }

            // Save paths in DB (for reference)
            await _meetingRepository.UpdateAssignmentAttachmentsAsync(itemId, filePaths);

            return Ok(new { itemId, filePaths }); // Frontend uses these to call secure GET
        }


        [HttpPost("{meetingId}/action-items/{itemId}/submission-attachments")]
        [Authorize]
        public async Task<IActionResult> UploadSubmissionAttachments(Guid itemId, List<IFormFile> files)
        {
            var item = await dbContext.ActionItems
                .Include(ai => ai.Meeting)
                .FirstOrDefaultAsync(ai => ai.Id == itemId);
            if (item == null)
                return NotFound("Action item not found.");
            if (item.AssignedToUserId != GetUserId())
                return StatusCode(403, "Access denied");
            if (files == null || !files.Any())
                return BadRequest("No files uploaded.");
            if (files.Count > 5)
                return BadRequest("You can upload a maximum of 5 files.");
            if (files.Any(f => f.Length > 5 * 1024 * 1024))
                return BadRequest("Each file must be less than 5MB.");
            if (files.Any(f => !new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".txt" }
                .Contains(Path.GetExtension(f.FileName).ToLower())))
                return BadRequest("Only .jpg, .jpeg, .png, .pdf, .docx, .xlsx, .txt files are allowed.");

            // Private folder outside wwwroot
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "upload", "action-items", itemId.ToString(), "submission");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePaths = new List<string>();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Relative path for secure GET endpoint
                var relativePath = Path.Combine("action-items", itemId.ToString(), "submission", fileName)
                                        .Replace("\\", "/");
                filePaths.Add(relativePath);
            }

            // Save paths in DB
            await _meetingRepository.UpdateSubmissionAttachmentsAsync(itemId, filePaths);

            return Ok(new { itemId, filePaths }); // Frontend uses these to call secure GET
        }

        [HttpPost("{meetingId}/attachments")]
        [Authorize]
        public async Task<IActionResult> UploadMeetingAttachments(Guid meetingId, List<IFormFile> files, [FromServices] IWebHostEnvironment env)
        {
            var meeting = await _meetingRepository.GetByIdAsync(meetingId);
            if (meeting == null)
                return NotFound("Meeting not found.");

            if (meeting.UserId != GetUserId())
                return StatusCode(403, "Access denied");

            if (files == null || !files.Any())
                return BadRequest("No files uploaded.");

            if (files.Count > 5)
                return BadRequest("You can upload a maximum of 5 files.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".txt" };

            // Save files outside wwwroot in a private "upload" folder
            var uploadsFolder = Path.Combine(env.ContentRootPath, "upload", "meetings", meetingId.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                    return BadRequest("One or more files are empty.");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest($"Invalid file type: {file.FileName}. Allowed: {string.Join(", ", allowedExtensions)}");

                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest($"File {file.FileName} exceeds the 5MB limit.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Store relative path for your repository, e.g. "meetings/{meetingId}/{guid}.ext"
                var relativePath = Path.Combine("meetings", meetingId.ToString(), fileName)
                                          .Replace(Path.DirectorySeparatorChar, '/');
                uploadedFiles.Add(relativePath);
            }

            // Save to repository (replaces old attachments and deletes old files)
            var updatedMeeting = await _meetingRepository.UpdateAttachmentAsync(meetingId, uploadedFiles);

            return Ok(new
            {
                MeetingId = updatedMeeting!.Id,
                AttachmentUrls = updatedMeeting.AttachmentUrls // these will now point to your secure GET endpoint
            });
        }
        private bool IsCreator(Meeting meeting, Guid userId) => meeting.UserId == userId;
        private bool IsCreatorOrInvitee(Meeting meeting, Guid userId) => IsCreator(meeting, userId) || meeting.Invitees.Any(i => i.UserId == userId && i.Attendance == "Accepted");
        private bool IsNoteOwner(Note note, Guid userId) => note.CreatedByUserId == userId;
        private bool IsAttachmentUploader(Attachment attachment, Guid userId) => attachment.UploadedByUserId == userId;
        private bool IsRoomFull(Room room, Meeting meeting)
        {
            logger.LogError(room.Capacity + " " + meeting.Invitees.Count());
            return room == null || meeting.Invitees.Count() >= room.Capacity;
        }
        private async Task<bool> IsRoomBookedAsync(Guid? roomId, Guid meetingId, DateTime startTime, DateTime endTime)
        {
            var meetings = await _meetingRepository.GetAllAsync();
            return meetings.Any(m => m.RoomId == roomId && m.StartTime < endTime && m.EndTime > startTime && m.Status != "Cancelled" && m.Id != meetingId);
        }
        private async Task<Meeting> EnsureMeetingAsync(Guid meetingId)
        {
            return await _meetingRepository.GetByIdAsync(meetingId);
        }


    }
}
