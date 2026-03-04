using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ToDoAIService");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DaySchedules",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaySchedules_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "States",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SleepMinutes = table.Column<int>(type: "integer", nullable: false),
                    EnergyLevel = table.Column<int>(type: "integer", nullable: false),
                    StressLevel = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEntityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                    table.ForeignKey(
                        name: "FK_States_Users_UserEntityId",
                        column: x => x.UserEntityId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    ComplexityLevel = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WorkStatus = table.Column<int>(type: "integer", nullable: false),
                    UserIdId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_UserIdId",
                        column: x => x.UserIdId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DayScheduleEntityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_DaySchedules_DayScheduleEntityId",
                        column: x => x.DayScheduleEntityId,
                        principalSchema: "ToDoAIService",
                        principalTable: "DaySchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Schedules_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskExecutions",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActualMinutes = table.Column<int>(type: "integer", nullable: false),
                    EnergyAfter = table.Column<int>(type: "integer", nullable: false),
                    StressAfter = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskExecutions_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DaySchedules_UserId",
                schema: "ToDoAIService",
                table: "DaySchedules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DayScheduleEntityId",
                schema: "ToDoAIService",
                table: "Schedules",
                column: "DayScheduleEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_TaskId",
                schema: "ToDoAIService",
                table: "Schedules",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_States_UserEntityId",
                schema: "ToDoAIService",
                table: "States",
                column: "UserEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutions_TaskId",
                schema: "ToDoAIService",
                table: "TaskExecutions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserIdId",
                schema: "ToDoAIService",
                table: "Tasks",
                column: "UserIdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules",
                schema: "ToDoAIService");

            migrationBuilder.DropTable(
                name: "States",
                schema: "ToDoAIService");

            migrationBuilder.DropTable(
                name: "TaskExecutions",
                schema: "ToDoAIService");

            migrationBuilder.DropTable(
                name: "DaySchedules",
                schema: "ToDoAIService");

            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "ToDoAIService");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "ToDoAIService");
        }
    }
}
