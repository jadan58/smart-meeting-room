using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Models;
using System;

namespace SmartMeetingRoomAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<RoomFeature> RoomFeatures { get; set; }
        public DbSet<RecurringBooking> RecurringBookings { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Invitee> Invitees { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<ActionItem> ActionItems { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ------------------------------
            // Meeting → User (Organizer)
            // ------------------------------
            builder.Entity<Meeting>()
                .HasOne(m => m.User)
                .WithMany(u => u.OrganizedMeetings)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade); // prevent multiple cascade paths

            // ------------------------------
            // Meeting → Room
            // ------------------------------
            builder.Entity<Meeting>()
                .HasOne(m => m.Room)
                .WithMany(r => r.Meetings)
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // ------------------------------
            // RoomFeature → Room & Feature (Many-to-many)
            // ------------------------------
            builder.Entity<RoomFeature>()
                .HasOne(rf => rf.Room)
                .WithMany(r => r.RoomFeatures)
                .HasForeignKey(rf => rf.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoomFeature>()
                .HasOne(rf => rf.Feature)
                .WithMany(f => f.RoomFeatures)
                .HasForeignKey(rf => rf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------
            // Invitee → Meeting
            // ------------------------------
            builder.Entity<Invitee>()
                .HasOne(i => i.Meeting)
                .WithMany(m => m.Invitees)
                .HasForeignKey(i => i.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------
            // ActionItem → Meeting
            // ------------------------------
            builder.Entity<ActionItem>()
                .HasOne(ai => ai.Meeting)
                .WithMany(m => m.ActionItems)
                .HasForeignKey(ai => ai.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------
            // Note → Meeting
            // ------------------------------
            builder.Entity<Note>()
                .HasOne(n => n.Meeting)
                .WithMany(m => m.Notes)
                .HasForeignKey(n => n.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------------------
            // Invitee → User (Invited user)
            // ------------------------------
            builder.Entity<Invitee>()
                .HasOne(i => i.User)
                .WithMany() // no collection needed on ApplicationUser
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
