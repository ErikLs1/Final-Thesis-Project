using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.EF.Migrations
{
    /// <inheritdoc />
    public partial class AuditIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ui_translation_audit_log_LanguageId",
                table: "ui_translation_audit_log");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_ActionType_ChangedAt",
                table: "ui_translation_audit_log",
                columns: new[] { "ActionType", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_ChangedAt",
                table: "ui_translation_audit_log",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_ChangedBy_ChangedAt",
                table: "ui_translation_audit_log",
                columns: new[] { "ChangedBy", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_LanguageId_ChangedAt",
                table: "ui_translation_audit_log",
                columns: new[] { "LanguageId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ui_translation_audit_log_ActionType_ChangedAt",
                table: "ui_translation_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_ui_translation_audit_log_ChangedAt",
                table: "ui_translation_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_ui_translation_audit_log_ChangedBy_ChangedAt",
                table: "ui_translation_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_ui_translation_audit_log_LanguageId_ChangedAt",
                table: "ui_translation_audit_log");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_LanguageId",
                table: "ui_translation_audit_log",
                column: "LanguageId");
        }
    }
}
