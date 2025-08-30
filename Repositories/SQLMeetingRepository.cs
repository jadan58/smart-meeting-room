using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Repositories
{
    public class SqlMeetingRepository : IMeetingRepository
    {
        private readonly AppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment env;

        public SqlMeetingRepository(AppDbContext dbContext, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.env = env;
        }
        //---------- Meetings ---------
        public async Task<List<Meeting>> GetAllAsync()
        {
            return await dbContext.Meetings.ToListAsync();
        }


        public async Task<Meeting?> GetByIdAsync(Guid id)
        {
            return await dbContext.Meetings
                .Include(m => m.Notes)
                .Include(m => m.ActionItems)
                .Include(m => m.Invitees)
                .Include(m => m.RecurringBooking)
                .Include(m => m.NextMeeting)
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Meeting> AddAsync(Meeting meeting)
        {
            await dbContext.Meetings.AddAsync(meeting);
            await dbContext.SaveChangesAsync();
            return meeting;
        }

        public async Task<Meeting?> UpdateAsync(Guid id, Meeting updatedMeeting)
        {
            var existing = await dbContext.Meetings.FindAsync(id);
            if (existing == null) return null;

            // update scalar properties
            existing.Title = updatedMeeting.Title;
            existing.Agenda = updatedMeeting.Agenda;
            existing.StartTime = updatedMeeting.StartTime;
            existing.EndTime = updatedMeeting.EndTime;
            existing.Status = updatedMeeting.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.RoomId = updatedMeeting.RoomId;
            existing.UserId = updatedMeeting.UserId;
            existing.RecurringBookingId = updatedMeeting.RecurringBookingId;
            existing.NextMeetingId = updatedMeeting.NextMeetingId;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<Meeting?> DeleteAsync(Guid id)
        {
            var meeting = await dbContext.Meetings.FindAsync(id);
            if (meeting == null) return null;

            dbContext.Meetings.Remove(meeting);
            await dbContext.SaveChangesAsync();
            return meeting;
        }

        // ---------- Notes ----------

        public async Task<Note?> AddNoteAsync(Guid meetingId, Note note)
        {
            // ensure parent meeting exists
            var meeting = await dbContext.Meetings.FindAsync(meetingId);
            if (meeting == null) return null;

            note.MeetingId = meetingId;
            note.CreatedAt = DateTime.UtcNow;
            await dbContext.Notes.AddAsync(note);
            await dbContext.SaveChangesAsync();
            return note;
        }

        public async Task<Note?> UpdateNoteAsync(Guid meetingId, Guid noteId, Note updatedNote)
        {
            var note = await dbContext.Notes
                .FirstOrDefaultAsync(n => n.Id == noteId && n.MeetingId == meetingId);
            if (note == null) return null;

            note.Content = updatedNote.Content;
            await dbContext.SaveChangesAsync();
            return note;
        }

        public async Task<Note?> DeleteNoteAsync(Guid meetingId, Guid noteId)
        {
            var note = await dbContext.Notes
                .FirstOrDefaultAsync(n => n.Id == noteId && n.MeetingId == meetingId);
            if (note == null) return null;

            dbContext.Notes.Remove(note);
            await dbContext.SaveChangesAsync();
            return note;
        }

        // ---------- Action Items ----------

        public async Task<ActionItem?> AddActionItemAsync(Guid meetingId, ActionItem actionItem)
        {
            var meeting = await dbContext.Meetings.FindAsync(meetingId);
            if (meeting == null) return null;

            actionItem.MeetingId = meetingId;
            await dbContext.ActionItems.AddAsync(actionItem);
            await dbContext.SaveChangesAsync();
            return actionItem;
        }

        public async Task<ActionItem?> UpdateActionItemAsync(Guid meetingId, Guid actionItemId, ActionItem updatedItem)
        {
            var item = await dbContext.ActionItems
                .FirstOrDefaultAsync(ai => ai.Id == actionItemId && ai.MeetingId == meetingId);
            if (item == null) return null;

            item.Description = updatedItem.Description;
            item.Deadline = updatedItem.Deadline;
            item.Status = updatedItem.Status;
            item.Type = updatedItem.Type;
            item.AssignedToUserId = updatedItem.AssignedToUserId;
            await dbContext.SaveChangesAsync();
            return item;
        }

        public async Task<ActionItem?> DeleteActionItemAsync(Guid meetingId, Guid actionItemId)
        {
            var item = await dbContext.ActionItems
                .FirstOrDefaultAsync(ai => ai.Id == actionItemId && ai.MeetingId == meetingId);
            if (item == null) return null;

            dbContext.ActionItems.Remove(item);
            await dbContext.SaveChangesAsync();
            return item;
        }

        // ---------- Invitees ----------

        public async Task<Invitee?> AddInviteeAsync(Guid meetingId, Invitee invitee)
        {
            var meeting = await dbContext.Meetings.FindAsync(meetingId);
            if (meeting == null) return null;

            invitee.MeetingId = meetingId;
            var user = await userManager.FindByIdAsync(invitee.UserId.ToString());
            if (user == null)
                return null;

            // Ensure InvitedMeetings is loaded and not null
            if (user.InvitedMeetings == null)
            {
                dbContext.Entry(user).Collection(u => u.InvitedMeetings).Load();
                if (user.InvitedMeetings == null)
                    user.InvitedMeetings = new List<Meeting>();
            }

            user.InvitedMeetings.Add(meeting);

            await dbContext.Invitees.AddAsync(invitee);
            await dbContext.SaveChangesAsync();
            return invitee;
        }

        public async Task<Invitee?> DeleteInviteeAsync(Guid inviteId)
        {
            var invite = await dbContext.Invitees
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return null;
            // Remove meeting from user's InvitedMeetings list if loaded
            var user = await dbContext.Users.FindAsync(invite.UserId);
            if (user != null && user.InvitedMeetings != null)
            {
                var meeting = await dbContext.Meetings.FindAsync(invite.MeetingId);
                if (meeting != null)
                {
                    user.InvitedMeetings.Remove(meeting);
                }
            }

            dbContext.Invitees.Remove(invite);
            await dbContext.SaveChangesAsync();
            return invite;
        }

        // ---------- Attachments ----------

        public async Task<Attachment?> AddAttachmentAsync(Guid meetingId, Attachment attachment)
        {
            var meeting = await dbContext.Meetings.FindAsync(meetingId);
            if (meeting == null) return null;

            attachment.MeetingId = meetingId;
            attachment.UploadedAt = DateTime.UtcNow;
            await dbContext.Attachments.AddAsync(attachment);
            await dbContext.SaveChangesAsync();
            return attachment;
        }

        public async Task<Attachment?> DeleteAttachmentAsync(Guid meetingId, Guid attachmentId)
        {
            var attachment = await dbContext.Attachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.MeetingId == meetingId);
            if (attachment == null) return null;

            dbContext.Attachments.Remove(attachment);
            await dbContext.SaveChangesAsync();
            return attachment;
        }

        async Task IMeetingRepository.AddRangeAsync(List<Meeting> meetings)
        {
            foreach (var meeting in meetings)
            {
                var m = await dbContext.Meetings.AddAsync(meeting);
            }
            await dbContext.SaveChangesAsync();
        }
        public async Task<Invitee?> UpdateInviteeAsync(Guid inviteId, Invitee updatedInvitee)
        {
            var invite = await dbContext.Invitees
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return null;
            invite.Status = updatedInvitee.Status;
            invite.Attendance = updatedInvitee.Attendance;

            await dbContext.SaveChangesAsync();
            return invite;
        }

        public async Task UpdateAssignmentAttachmentsAsync(Guid itemId, List<string> newUrls)
        {
            var item = await dbContext.ActionItems.FindAsync(itemId);
            if (item == null) return;

            // Delete old files
            if (item.AssignmentAttachmentsUrl != null)
            {
                foreach (var relativePath in item.AssignmentAttachmentsUrl)
                {
                    var absolutePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                                    relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(absolutePath))
                        File.Delete(absolutePath);
                }
            }

            // Replace with new list
            item.AssignmentAttachmentsUrl = newUrls ?? new List<string>();
            await dbContext.SaveChangesAsync();
        }

        // Replace submission attachments
        public async Task UpdateSubmissionAttachmentsAsync(Guid itemId, List<string> newUrls)
        {
            var item = await dbContext.ActionItems.FindAsync(itemId);
            if (item == null) return;

            // Delete old files
            if (item.SubmissionAttachmentsUrl != null)
            {
                foreach (var relativePath in item.SubmissionAttachmentsUrl)
                {
                    var absolutePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                                    relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(absolutePath))
                        File.Delete(absolutePath);
                }
            }

            // Replace with new list
            item.SubmissionAttachmentsUrl = newUrls ?? new List<string>();
            await dbContext.SaveChangesAsync();
        }
        public async Task<Meeting?> UpdateAttachmentAsync(Guid meetingId, List<string> fileUrls)
        {
            var meeting = await dbContext.Meetings.FindAsync(meetingId);
            if (meeting == null) return null;

            // Delete old files
            if (meeting.AttachmentUrls?.Any() == true)
            {
                foreach (var relativePath in meeting.AttachmentUrls)
                {
                    var absolutePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                                    relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(absolutePath))
                        File.Delete(absolutePath);
                }
            }

            // Replace with new list
            meeting.AttachmentUrls = fileUrls ?? new List<string>();
            await dbContext.SaveChangesAsync();
            return meeting;
        }

    }
}