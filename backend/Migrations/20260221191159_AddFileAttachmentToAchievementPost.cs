using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFileAttachmentToAchievementPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attachment_filename",
                table: "achievement_posts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attachment_public_id",
                table: "achievement_posts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attachment_url",
                table: "achievement_posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attachment_filename",
                table: "achievement_posts");

            migrationBuilder.DropColumn(
                name: "attachment_public_id",
                table: "achievement_posts");

            migrationBuilder.DropColumn(
                name: "attachment_url",
                table: "achievement_posts");
        }
    }
}
