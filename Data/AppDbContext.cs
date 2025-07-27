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

            // ✅ Self-referencing relationship for NextMeeting
            builder.Entity<Meeting>()
                .HasOne(m => m.NextMeeting)
                .WithOne()
                .HasForeignKey<Meeting>(m => m.NextMeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Many-to-many join RoomFeature
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

            // ✅ Prevent multiple cascade paths: use Restrict for Meeting relations
            builder.Entity<Invitee>()
                .HasOne(i => i.Meeting)
                .WithMany(m => m.Invitees)
                .HasForeignKey(i => i.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ActionItem>()
                .HasOne(ai => ai.Meeting)
                .WithMany(m => m.ActionItems)
                .HasForeignKey(ai => ai.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Note>()
                .HasOne(n => n.Meeting)
                .WithMany(m => m.Notes)
                .HasForeignKey(n => n.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attachment>()
                .HasOne(a => a.Meeting)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
