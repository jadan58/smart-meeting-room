using SmartMeetingRoomAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Repositories
{
    public interface IMeetingRepository
    {
        Task<IEnumerable<Meeting>> GetAllAsync();
        Task<Meeting?> GetByIdAsync(Guid id);
        Task<Meeting> AddAsync(Meeting meeting);
        Task<Meeting?> UpdateAsync(Guid id, Meeting meeting);
        Task<Meeting?> DeleteAsync(Guid id);

        // Notes
        Task<Note?> AddNoteAsync(Guid meetingId, Note note);
        Task<Note?> UpdateNoteAsync(Guid meetingId, Guid noteId, Note note);
        Task<Note?> DeleteNoteAsync(Guid meetingId, Guid noteId);

        // Action Items
        Task<ActionItem?> AddActionItemAsync(Guid meetingId, ActionItem actionItem);
        Task<ActionItem?> UpdateActionItemAsync(Guid meetingId, Guid actionItemId, ActionItem actionItem);
        Task<ActionItem?> DeleteActionItemAsync(Guid meetingId, Guid actionItemId);

        // Invitees
        Task<Invitee?> AddInviteeAsync(Guid meetingId, Invitee invitee);
        Task<Invitee?> DeleteInviteeAsync(Guid meetingId, Guid inviteeId);

        // Attachments
        Task<Attachment?> AddAttachmentAsync(Guid meetingId, Attachment attachment);
        Task<Attachment?> DeleteAttachmentAsync(Guid meetingId, Guid attachmentId);

    }
}
