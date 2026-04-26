using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntitiesForEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                schema: "ToDoAIService",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailConfirmations",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailConfirmations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResets",
                schema: "ToDoAIService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResets_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "ToDoAIService",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmations_ExpiresAt",
                schema: "ToDoAIService",
                table: "EmailConfirmations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmations_UserId_CodeHash",
                schema: "ToDoAIService",
                table: "EmailConfirmations",
                columns: new[] { "UserId", "CodeHash" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmations_UserId_SentAt",
                schema: "ToDoAIService",
                table: "EmailConfirmations",
                columns: new[] { "UserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_ExpiresAt",
                schema: "ToDoAIService",
                table: "PasswordResets",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId_CodeHash",
                schema: "ToDoAIService",
                table: "PasswordResets",
                columns: new[] { "UserId", "CodeHash" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId_SentAt",
                schema: "ToDoAIService",
                table: "PasswordResets",
                columns: new[] { "UserId", "SentAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
