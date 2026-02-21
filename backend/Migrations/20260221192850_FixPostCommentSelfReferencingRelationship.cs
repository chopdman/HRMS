using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixPostCommentSelfReferencingRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing self-referencing foreign key if it was created with CASCADE
            migrationBuilder.DropForeignKey(
                name: "FK_post_comments_post_comments_fk_parent_comment_id",
                table: "post_comments"
            );

            // Re-create it with NO ACTION to prevent cascade path issues
            migrationBuilder.AddForeignKey(
                name: "FK_post_comments_post_comments_fk_parent_comment_id",
                table: "post_comments",
                column: "fk_parent_comment_id",
                principalTable: "post_comments",
                principalColumn: "pk_comment_id",
                onDelete: ReferentialAction.NoAction
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key we just created
            migrationBuilder.DropForeignKey(
                name: "FK_post_comments_post_comments_fk_parent_comment_id",
                table: "post_comments"
            );

            // This will rollback to the original (which may have had the same name but different settings)
            // The original migration handles the recreation if needed
        }
    }
}
