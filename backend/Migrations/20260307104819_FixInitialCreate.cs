using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Migrations
{
    /// <inheritdoc />
    public partial class FixInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_DaySchedules_DayScheduleEntityId",
                schema: "ToDoAIService",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Tasks_TaskId",
                schema: "ToDoAIService",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_States_Users_UserEntityId",
                schema: "ToDoAIService",
                table: "States");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_UserIdId",
                schema: "ToDoAIService",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_States_UserEntityId",
                schema: "ToDoAIService",
                table: "States");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_DayScheduleEntityId",
                schema: "ToDoAIService",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "UserEntityId",
                schema: "ToDoAIService",
                table: "States");

            migrationBuilder.DropColumn(
                name: "DayScheduleEntityId",
                schema: "ToDoAIService",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "UserIdId",
                schema: "ToDoAIService",
                table: "Tasks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_UserIdId",
                schema: "ToDoAIService",
                table: "Tasks",
                newName: "IX_Tasks_UserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TaskId",
                schema: "ToDoAIService",
                table: "Schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Start",
                schema: "ToDoAIService",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "End",
                schema: "ToDoAIService",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "DayScheduleId",
                schema: "ToDoAIService",
                table: "Schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_States_UserId",
                schema: "ToDoAIService",
                table: "States",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DayScheduleId",
                schema: "ToDoAIService",
                table: "Schedules",
                column: "DayScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_DaySchedules_DayScheduleId",
                schema: "ToDoAIService",
                table: "Schedules",
                column: "DayScheduleId",
                principalSchema: "ToDoAIService",
                principalTable: "DaySchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Tasks_TaskId",
                schema: "ToDoAIService",
                table: "Schedules",
                column: "TaskId",
                principalSchema: "ToDoAIService",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_States_Users_UserId",
                schema: "ToDoAIService",
                table: "States",
                column: "UserId",
                principalSchema: "ToDoAIService",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_UserId",
                schema: "ToDoAIService",
                table: "Tasks",
                column: "UserId",
                principalSchema: "ToDoAIService",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
