using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class editappuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Meetings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ApplicationUserId",
                table: "Meetings",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_ApplicationUserId",
                table: "Meetings",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_ApplicationUserId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_ApplicationUserId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Meetings");
        }
    }
}
