using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDayScheduleTimeZoneOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeZoneOffsetMinutes",
                schema: "ToDoAIService",
                table: "DaySchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
