using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitees_Meetings_MeetingId",
                table: "Invitees");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitees_Meetings_MeetingId",
                table: "Invitees",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitees_Meetings_MeetingId",
                table: "Invitees");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitees_Meetings_MeetingId",
                table: "Invitees",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
