using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskExecutionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                schema: "ToDoAIService",
                table: "TaskExecutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutions_ScheduleId",
                schema: "ToDoAIService",
                table: "TaskExecutions",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskExecutions_Schedules_ScheduleId",
                schema: "ToDoAIService",
                table: "TaskExecutions",
                column: "ScheduleId",
                principalSchema: "ToDoAIService",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
