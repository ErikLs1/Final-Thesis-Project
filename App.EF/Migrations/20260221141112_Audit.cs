using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.EF.Migrations
{
    /// <inheritdoc />
    public partial class Audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "ui_translation_audit_log",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedAt",
                table: "ui_translation_audit_log",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "ui_translation_audit_log",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewContent",
                table: "ui_translation_audit_log",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewState",
                table: "ui_translation_audit_log",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldContent",
                table: "ui_translation_audit_log",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldState",
                table: "ui_translation_audit_log",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "ui_translation_audit_log");

            migrationBuilder.DropColumn(
                name: "ChangedAt",
                table: "ui_translation_audit_log");

            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "ui_translation_audit_log");

            migrationBuilder.DropColumn(
                name: "NewContent",
                table: "ui_translation_audit_log");

            migrationBuilder.DropColumn(
                name: "NewState",
                table: "ui_translation_audit_log");

            migrationBuilder.DropColumn(
                name: "OldContent",
                table: "ui_translation_audit_log");

            migrationBuilder.DropColumn(
                name: "OldState",
                table: "ui_translation_audit_log");
        }
    }
}
