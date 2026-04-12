using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTaskDeadlineColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadlineAt",
                schema: "ToDoAIService",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
            
            migrationBuilder.AddColumn<int>(
                name: "MotivationLevel",
                schema: "ToDoAIService",
                table: "States",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ConcentrationLevel",
                schema: "ToDoAIService",
                table: "States",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
