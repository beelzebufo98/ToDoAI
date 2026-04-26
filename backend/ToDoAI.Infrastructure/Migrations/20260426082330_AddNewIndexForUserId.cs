using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewIndexForUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId",
                schema: "ToDoAIService",
                table: "PasswordResets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfirmations_UserId",
                schema: "ToDoAIService",
                table: "EmailConfirmations",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}