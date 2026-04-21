using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskWorkSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualSpentMinutes",
                schema: "ToDoAIService",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskWorkSessions",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleBlockId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SpentMinutes = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWorkSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskWorkSessions_Schedules_ScheduleBlockId",
                        column: x => x.ScheduleBlockId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaskWorkSessions_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskWorkSessions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_ScheduleBlockId",
                schema: "ToDoAIService",
                table: "TaskWorkSessions",
                column: "ScheduleBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_TaskId_StartedAt",
                schema: "ToDoAIService",
                table: "TaskWorkSessions",
                columns: new[] { "TaskId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_UserId",
                schema: "ToDoAIService",
                table: "TaskWorkSessions",
                column: "UserId",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_UserId_StartedAt",
                schema: "ToDoAIService",
                table: "TaskWorkSessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_UserId_Status",
                schema: "ToDoAIService",
                table: "TaskWorkSessions",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}