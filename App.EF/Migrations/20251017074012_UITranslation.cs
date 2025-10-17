using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.EF.Migrations
{
    /// <inheritdoc />
    public partial class UITranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultLanguage",
                table: "languages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LanguageName",
                table: "languages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LanguageTag",
                table: "languages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ui_resource_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ResourceKey = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ui_resource_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ui_translation_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TranslationState = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ui_translation_versions", x => x.Id);
                    table.UniqueConstraint("AK_ui_translation_versions_LanguageId_ResourceKeyId_Id", x => new { x.LanguageId, x.ResourceKeyId, x.Id });
                    table.ForeignKey(
                        name: "FK_ui_translation_versions_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ui_translation_versions_ui_resource_keys_ResourceKeyId",
                        column: x => x.ResourceKeyId,
                        principalTable: "ui_resource_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ui_experiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TranslationVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExperimentName = table.Column<string>(type: "text", nullable: false),
                    Option = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ui_experiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ui_experiments_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ui_experiments_ui_resource_keys_ResourceKeyId",
                        column: x => x.ResourceKeyId,
                        principalTable: "ui_resource_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ui_experiments_ui_translation_versions_LanguageId_ResourceK~",
                        columns: x => new { x.LanguageId, x.ResourceKeyId, x.TranslationVersionId },
                        principalTable: "ui_translation_versions",
                        principalColumns: new[] { "LanguageId", "ResourceKeyId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ui_translation_audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TranslationVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivatedBy = table.Column<string>(type: "text", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeactivatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ui_translation_audit_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ui_translation_audit_log_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ui_translation_audit_log_ui_resource_keys_ResourceKeyId",
                        column: x => x.ResourceKeyId,
                        principalTable: "ui_resource_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ui_translation_audit_log_ui_translation_versions_Translatio~",
                        column: x => x.TranslationVersionId,
                        principalTable: "ui_translation_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ui_translations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TranslationVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ui_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ui_translations_languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ui_translations_ui_resource_keys_ResourceKeyId",
                        column: x => x.ResourceKeyId,
                        principalTable: "ui_resource_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ui_translations_ui_translation_versions_LanguageId_Resource~",
                        columns: x => new { x.LanguageId, x.ResourceKeyId, x.TranslationVersionId },
                        principalTable: "ui_translation_versions",
                        principalColumns: new[] { "LanguageId", "ResourceKeyId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_languages_LanguageTag",
                table: "languages",
                column: "LanguageTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ui_experiments_LanguageId_ResourceKeyId",
                table: "ui_experiments",
                columns: new[] { "LanguageId", "ResourceKeyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ui_experiments_LanguageId_ResourceKeyId_TranslationVersionId",
                table: "ui_experiments",
                columns: new[] { "LanguageId", "ResourceKeyId", "TranslationVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ui_experiments_ResourceKeyId",
                table: "ui_experiments",
                column: "ResourceKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ui_resource_keys_ResourceKey",
                table: "ui_resource_keys",
                column: "ResourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_LanguageId",
                table: "ui_translation_audit_log",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_ResourceKeyId",
                table: "ui_translation_audit_log",
                column: "ResourceKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_audit_log_TranslationVersionId",
                table: "ui_translation_audit_log",
                column: "TranslationVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_versions_LanguageId_ResourceKeyId",
                table: "ui_translation_versions",
                columns: new[] { "LanguageId", "ResourceKeyId" });

            migrationBuilder.CreateIndex(
                name: "IX_ui_translation_versions_ResourceKeyId",
                table: "ui_translation_versions",
                column: "ResourceKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ui_translations_LanguageId",
                table: "ui_translations",
                column: "LanguageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ui_translations_LanguageId_ResourceKeyId",
                table: "ui_translations",
                columns: new[] { "LanguageId", "ResourceKeyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ui_translations_LanguageId_ResourceKeyId_TranslationVersion~",
                table: "ui_translations",
                columns: new[] { "LanguageId", "ResourceKeyId", "TranslationVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ui_translations_ResourceKeyId",
                table: "ui_translations",
                column: "ResourceKeyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ui_experiments");

            migrationBuilder.DropTable(
                name: "ui_translation_audit_log");

            migrationBuilder.DropTable(
                name: "ui_translations");

            migrationBuilder.DropTable(
                name: "ui_translation_versions");

            migrationBuilder.DropTable(
                name: "ui_resource_keys");

            migrationBuilder.DropIndex(
                name: "IX_languages_LanguageTag",
                table: "languages");

            migrationBuilder.DropColumn(
                name: "IsDefaultLanguage",
                table: "languages");

            migrationBuilder.DropColumn(
                name: "LanguageName",
                table: "languages");

            migrationBuilder.DropColumn(
                name: "LanguageTag",
                table: "languages");
        }
    }
}
