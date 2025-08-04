using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMeetingNavigationProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitees_AspNetUsers_UserId",
                table: "Invitees");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_UserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Meetings_NextMeetingId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Rooms_RoomId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_NextMeetingId",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_NextMeetingId",
                table: "Meetings",
                column: "NextMeetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitees_AspNetUsers_UserId",
                table: "Invitees",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_UserId",
                table: "Meetings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Meetings_NextMeetingId",
                table: "Meetings",
                column: "NextMeetingId",
                principalTable: "Meetings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Rooms_RoomId",
                table: "Meetings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitees_AspNetUsers_UserId",
                table: "Invitees");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_UserId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Meetings_NextMeetingId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Rooms_RoomId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_NextMeetingId",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_NextMeetingId",
                table: "Meetings",
                column: "NextMeetingId",
                unique: true,
                filter: "[NextMeetingId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitees_AspNetUsers_UserId",
                table: "Invitees",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_UserId",
                table: "Meetings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Meetings_NextMeetingId",
                table: "Meetings",
                column: "NextMeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Rooms_RoomId",
                table: "Meetings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
