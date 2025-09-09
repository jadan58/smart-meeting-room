using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class OnlineMeetignLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "onlineLink",
                table: "Meetings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onlineLink",
                table: "Meetings");
        }
    }
}
