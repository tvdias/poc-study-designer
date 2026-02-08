using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionBankItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuestionText = table.Column<string>(type: "text", nullable: true),
                    Classification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDummy = table.Column<bool>(type: "boolean", nullable: false),
                    QuestionTitle = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StatusReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Methodology = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataQualityTag = table.Column<string>(type: "text", nullable: true),
                    RowSortOrder = table.Column<int>(type: "integer", nullable: true),
                    ColumnSortOrder = table.Column<int>(type: "integer", nullable: true),
                    AnswerMin = table.Column<int>(type: "integer", nullable: true),
                    AnswerMax = table.Column<int>(type: "integer", nullable: true),
                    QuestionFormatDetails = table.Column<string>(type: "text", nullable: true),
                    ScraperNotes = table.Column<string>(type: "text", nullable: true),
                    CustomNotes = table.Column<string>(type: "text", nullable: true),
                    MetricGroup = table.Column<string>(type: "text", nullable: true),
                    TableTitle = table.Column<string>(type: "text", nullable: true),
                    QuestionRationale = table.Column<string>(type: "text", nullable: true),
                    SingleOrMulticode = table.Column<string>(type: "text", nullable: true),
                    ManagedListReferences = table.Column<string>(type: "text", nullable: true),
                    IsTranslatable = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    IsQuestionActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsQuestionOutOfUse = table.Column<bool>(type: "boolean", nullable: false),
                    AnswerRestrictionMin = table.Column<int>(type: "integer", nullable: true),
                    AnswerRestrictionMax = table.Column<int>(type: "integer", nullable: true),
                    RestrictionDataType = table.Column<string>(type: "text", nullable: true),
                    RestrictedToClient = table.Column<string>(type: "text", nullable: true),
                    AnswerTypeCode = table.Column<string>(type: "text", nullable: true),
                    IsAnswerRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ScalePoint = table.Column<string>(type: "text", nullable: true),
                    ScaleType = table.Column<string>(type: "text", nullable: true),
                    DisplayType = table.Column<string>(type: "text", nullable: true),
                    InstructionText = table.Column<string>(type: "text", nullable: true),
                    ParentQuestionId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuestionFacet = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionBankItems_QuestionBankItems_ParentQuestionId",
                        column: x => x.ParentQuestionId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AnswerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AnswerLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    IsExclusive = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CustomProperty = table.Column<string>(type: "text", nullable: true),
                    Facets = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionAnswers_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswers_QuestionBankItemId",
                table: "QuestionAnswers",
                column: "QuestionBankItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_ParentQuestionId",
                table: "QuestionBankItems",
                column: "ParentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_VariableName_Version",
                table: "QuestionBankItems",
                columns: new[] { "VariableName", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionAnswers");

            migrationBuilder.DropTable(
                name: "QuestionBankItems");
        }
    }
}
