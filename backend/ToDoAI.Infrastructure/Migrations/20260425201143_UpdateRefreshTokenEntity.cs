using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefreshTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RefreshSessions_UserId",
                schema: "ToDoAIService",
                table: "RefreshSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshSessions_Users_UserId",
                schema: "ToDoAIService",
                table: "RefreshSessions",
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
