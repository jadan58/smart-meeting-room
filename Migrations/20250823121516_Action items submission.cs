using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingRoomAPI.Migrations
{
    /// <inheritdoc />
    public partial class Actionitemssubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Judgment",
                table: "ActionItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Judgment",
                table: "ActionItems");
        }
    }
}
