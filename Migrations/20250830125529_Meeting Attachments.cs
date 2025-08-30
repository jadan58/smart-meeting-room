using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class MeetingAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Meetings_MeetingId",
                table: "Attachments");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrls",
                table: "Meetings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Meetings_MeetingId",
                table: "Attachments",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Meetings_MeetingId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AttachmentUrls",
                table: "Meetings");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Meetings_MeetingId",
                table: "Attachments",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
