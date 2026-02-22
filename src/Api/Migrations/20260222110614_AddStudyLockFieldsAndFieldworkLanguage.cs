using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyLockFieldsAndFieldworkLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EditCustomAnswerCode",
                table: "StudyQuestionnaireLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockAnswerCode",
                table: "StudyQuestionnaireLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FieldworkLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldworkMarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LanguageName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldworkLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldworkLanguages_FieldworkMarkets_FieldworkMarketId",
                        column: x => x.FieldworkMarketId,
                        principalTable: "FieldworkMarkets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldworkLanguages_Studies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldworkLanguages_FieldworkMarketId",
                table: "FieldworkLanguages",
                column: "FieldworkMarketId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldworkLanguages_StudyId_FieldworkMarketId_LanguageCode",
                table: "FieldworkLanguages",
                columns: new[] { "StudyId", "FieldworkMarketId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldworkLanguages");

            migrationBuilder.DropColumn(
                name: "EditCustomAnswerCode",
                table: "StudyQuestionnaireLines");

            migrationBuilder.DropColumn(
                name: "LockAnswerCode",
                table: "StudyQuestionnaireLines");
        }
    }
}
