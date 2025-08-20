using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class Invitees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Invitees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Invitees");
        }
    }
}
