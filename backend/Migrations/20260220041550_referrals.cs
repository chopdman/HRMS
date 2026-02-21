using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class referrals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_logs",
                columns: table => new
                {
                    pk_email_log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recipient_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fk_job_id = table.Column<long>(type: "bigint", nullable: true),
                    fk_referral_id = table.Column<long>(type: "bigint", nullable: true),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_logs", x => x.pk_email_log_id);
                });

            migrationBuilder.CreateTable(
                name: "global_config",
                columns: table => new
                {
                    pk_config_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    config_field = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    config_value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    related_table = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_config", x => x.pk_config_id);
                });

            migrationBuilder.CreateTable(
                name: "job_openings",
                columns: table => new
                {
                    pk_job_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    job_type = table.Column<int>(type: "int", nullable: false),
                    experience_required = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    job_summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    job_description_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    hr_owner_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cv_reviewer_emails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fk_posted_by = table.Column<long>(type: "bigint", nullable: true),
                    posted_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_openings", x => x.pk_job_id);
                    table.ForeignKey(
                        name: "FK_job_openings_users_fk_posted_by",
                        column: x => x.fk_posted_by,
                        principalTable: "users",
                        principalColumn: "pk_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "job_shares",
                columns: table => new
                {
                    pk_share_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fk_job_id = table.Column<long>(type: "bigint", nullable: false),
                    fk_shared_by = table.Column<long>(type: "bigint", nullable: false),
                    recipient_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    shared_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_shares", x => x.pk_share_id);
                    table.ForeignKey(
                        name: "FK_job_shares_job_openings_fk_job_id",
                        column: x => x.fk_job_id,
                        principalTable: "job_openings",
                        principalColumn: "pk_job_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_shares_users_fk_shared_by",
                        column: x => x.fk_shared_by,
                        principalTable: "users",
                        principalColumn: "pk_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "referrals",
                columns: table => new
                {
                    pk_referral_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fk_job_id = table.Column<long>(type: "bigint", nullable: false),
                    fk_referred_by = table.Column<long>(type: "bigint", nullable: false),
                    friend_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    friend_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cv_file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    hr_recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status_updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fk_status_updated_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referrals", x => x.pk_referral_id);
                    table.ForeignKey(
                        name: "FK_referrals_job_openings_fk_job_id",
                        column: x => x.fk_job_id,
                        principalTable: "job_openings",
                        principalColumn: "pk_job_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_referrals_users_fk_referred_by",
                        column: x => x.fk_referred_by,
                        principalTable: "users",
                        principalColumn: "pk_user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_referrals_users_fk_status_updated_by",
                        column: x => x.fk_status_updated_by,
                        principalTable: "users",
                        principalColumn: "pk_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "referral_status_logs",
                columns: table => new
                {
                    pk_referral_status_log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fk_referral_id = table.Column<long>(type: "bigint", nullable: false),
                    old_status = table.Column<int>(type: "int", nullable: true),
                    new_status = table.Column<int>(type: "int", nullable: false),
                    recipients_snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fk_changed_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_status_logs", x => x.pk_referral_status_log_id);
                    table.ForeignKey(
                        name: "FK_referral_status_logs_referrals_fk_referral_id",
                        column: x => x.fk_referral_id,
                        principalTable: "referrals",
                        principalColumn: "pk_referral_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_referral_status_logs_users_fk_changed_by",
                        column: x => x.fk_changed_by,
                        principalTable: "users",
                        principalColumn: "pk_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_global_config_config_field",
                table: "global_config",
                column: "config_field",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_openings_fk_posted_by",
                table: "job_openings",
                column: "fk_posted_by");

            migrationBuilder.CreateIndex(
                name: "IX_job_shares_fk_job_id",
                table: "job_shares",
                column: "fk_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_shares_fk_shared_by",
                table: "job_shares",
                column: "fk_shared_by");

            migrationBuilder.CreateIndex(
                name: "IX_referral_status_logs_fk_changed_by",
                table: "referral_status_logs",
                column: "fk_changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_referral_status_logs_fk_referral_id",
                table: "referral_status_logs",
                column: "fk_referral_id");

            migrationBuilder.CreateIndex(
                name: "IX_referrals_fk_job_id",
                table: "referrals",
                column: "fk_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_referrals_fk_referred_by",
                table: "referrals",
                column: "fk_referred_by");

            migrationBuilder.CreateIndex(
                name: "IX_referrals_fk_status_updated_by",
                table: "referrals",
                column: "fk_status_updated_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_logs");

            migrationBuilder.DropTable(
                name: "global_config");

            migrationBuilder.DropTable(
                name: "job_shares");

            migrationBuilder.DropTable(
                name: "referral_status_logs");

            migrationBuilder.DropTable(
                name: "referrals");

            migrationBuilder.DropTable(
                name: "job_openings");
        }
    }
}
