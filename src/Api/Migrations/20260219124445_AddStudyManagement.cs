using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasStudies",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStudyModifiedOn",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudyCount",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Studies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MasterStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VersionComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VersionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Studies_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Studies_Studies_MasterStudyId",
                        column: x => x.MasterStudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Studies_Studies_ParentStudyId",
                        column: x => x.ParentStudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudyQuestionnaireLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    VariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    QuestionTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QuestionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Classification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuestionRationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ScraperNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CustomNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowSortOrder = table.Column<int>(type: "integer", nullable: true),
                    ColumnSortOrder = table.Column<int>(type: "integer", nullable: true),
                    AnswerMin = table.Column<int>(type: "integer", nullable: true),
                    AnswerMax = table.Column<int>(type: "integer", nullable: true),
                    QuestionFormatDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDummy = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyQuestionnaireLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyQuestionnaireLines_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyQuestionnaireLines_Studies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyManagedListAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyQuestionnaireLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyManagedListAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyManagedListAssignments_ManagedLists_ManagedListId",
                        column: x => x.ManagedListId,
                        principalTable: "ManagedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyManagedListAssignments_Studies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyManagedListAssignments_StudyQuestionnaireLines_StudyQu~",
                        column: x => x.StudyQuestionnaireLineId,
                        principalTable: "StudyQuestionnaireLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyQuestionSubsetLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyQuestionnaireLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubsetDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyQuestionSubsetLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyQuestionSubsetLinks_ManagedLists_ManagedListId",
                        column: x => x.ManagedListId,
                        principalTable: "ManagedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyQuestionSubsetLinks_Studies_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Studies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyQuestionSubsetLinks_StudyQuestionnaireLines_StudyQuest~",
                        column: x => x.StudyQuestionnaireLineId,
                        principalTable: "StudyQuestionnaireLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyQuestionSubsetLinks_SubsetDefinitions_SubsetDefinition~",
                        column: x => x.SubsetDefinitionId,
                        principalTable: "SubsetDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Studies_MasterStudyId",
                table: "Studies",
                column: "MasterStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_ParentStudyId",
                table: "Studies",
                column: "ParentStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_ProjectId_MasterStudyId_VersionNumber",
                table: "Studies",
                columns: new[] { "ProjectId", "MasterStudyId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyManagedListAssignments_ManagedListId",
                table: "StudyManagedListAssignments",
                column: "ManagedListId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyManagedListAssignments_StudyId",
                table: "StudyManagedListAssignments",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyManagedListAssignments_StudyQuestionnaireLineId_Manage~",
                table: "StudyManagedListAssignments",
                columns: new[] { "StudyQuestionnaireLineId", "ManagedListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyQuestionnaireLines_QuestionBankItemId",
                table: "StudyQuestionnaireLines",
                column: "QuestionBankItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyQuestionnaireLines_StudyId_SortOrder",
                table: "StudyQuestionnaireLines",
                columns: new[] { "StudyId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyQuestionSubsetLinks_ManagedListId",
                table: "StudyQuestionSubsetLinks",
                column: "ManagedListId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyQuestionSubsetLinks_StudyId",
                table: "StudyQuestionSubsetLinks",
                column: "StudyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyQuestionSubsetLinks_StudyQuestionnaireLineId_ManagedLi~",
                table: "StudyQuestionSubsetLinks",
                columns: new[] { "StudyQuestionnaireLineId", "ManagedListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyQuestionSubsetLinks_SubsetDefinitionId",
                table: "StudyQuestionSubsetLinks",
                column: "SubsetDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyManagedListAssignments");

            migrationBuilder.DropTable(
                name: "StudyQuestionSubsetLinks");

            migrationBuilder.DropTable(
                name: "StudyQuestionnaireLines");

            migrationBuilder.DropTable(
                name: "Studies");

            migrationBuilder.DropColumn(
                name: "HasStudies",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastStudyModifiedOn",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StudyCount",
                table: "Projects");
        }
    }
}
