using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedEndDate",
                schema: "ToDoAIService",
                table: "Tasks");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActualStartDate",
                schema: "ToDoAIService",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
