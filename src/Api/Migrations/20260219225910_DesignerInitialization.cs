using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class DesignerInitialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommissioningMarketId = table.Column<Guid>(type: "uuid", nullable: true),
                    Methodology = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CostManagementEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    HasStudies = table.Column<bool>(type: "boolean", nullable: false),
                    StudyCount = table.Column<int>(type: "integer", nullable: false),
                    LastStudyModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Projects_CommissioningMarkets_CommissioningMarketId",
                        column: x => x.CommissioningMarketId,
                        principalTable: "CommissioningMarkets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Projects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ManagedLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagedLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagedLists_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    VariableName = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: true),
                    QuestionTitle = table.Column<string>(type: "text", nullable: true),
                    QuestionType = table.Column<string>(type: "text", nullable: true),
                    Classification = table.Column<string>(type: "text", nullable: true),
                    QuestionRationale = table.Column<string>(type: "text", nullable: true),
                    ScraperNotes = table.Column<string>(type: "text", nullable: true),
                    CustomNotes = table.Column<string>(type: "text", nullable: true),
                    RowSortOrder = table.Column<int>(type: "integer", nullable: true),
                    ColumnSortOrder = table.Column<int>(type: "integer", nullable: true),
                    AnswerMin = table.Column<int>(type: "integer", nullable: true),
                    AnswerMax = table.Column<int>(type: "integer", nullable: true),
                    QuestionFormatDetails = table.Column<string>(type: "text", nullable: true),
                    IsDummy = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireLines_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionnaireLines_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Studies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    FieldworkMarketId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaconomyJobNumber = table.Column<string>(type: "text", nullable: false),
                    ProjectOperationsUrl = table.Column<string>(type: "text", nullable: false),
                    ScripterNotes = table.Column<string>(type: "text", nullable: true),
                    MasterStudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Studies_FieldworkMarkets_FieldworkMarketId",
                        column: x => x.FieldworkMarketId,
                        principalTable: "FieldworkMarkets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "ManagedListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagedListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagedListItems_ManagedLists_ManagedListId",
                        column: x => x.ManagedListId,
                        principalTable: "ManagedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubsetDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SignatureHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsetDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubsetDefinitions_ManagedLists_ManagedListId",
                        column: x => x.ManagedListId,
                        principalTable: "ManagedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubsetDefinitions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionManagedLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionManagedLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionManagedLists_ManagedLists_ManagedListId",
                        column: x => x.ManagedListId,
                        principalTable: "ManagedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionManagedLists_QuestionnaireLines_QuestionnaireLineId",
                        column: x => x.QuestionnaireLineId,
                        principalTable: "QuestionnaireLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "QuestionSubsetLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubsetDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSubsetLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSubsetLinks_ManagedLists_ManagedListId",
                        column: x => x.ManagedListId,
                        principalTable: "ManagedLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSubsetLinks_QuestionnaireLines_QuestionnaireLineId",
                        column: x => x.QuestionnaireLineId,
                        principalTable: "QuestionnaireLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionSubsetLinks_SubsetDefinitions_SubsetDefinitionId",
                        column: x => x.SubsetDefinitionId,
                        principalTable: "SubsetDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubsetMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubsetDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedListItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsetMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubsetMemberships_ManagedListItems_ManagedListItemId",
                        column: x => x.ManagedListItemId,
                        principalTable: "ManagedListItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubsetMemberships_SubsetDefinitions_SubsetDefinitionId",
                        column: x => x.SubsetDefinitionId,
                        principalTable: "SubsetDefinitions",
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
                name: "IX_ManagedListItems_ManagedListId_Value",
                table: "ManagedListItems",
                columns: new[] { "ManagedListId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagedLists_ProjectId_Name",
                table: "ManagedLists",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId_Name",
                table: "Projects",
                columns: new[] { "ClientId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CommissioningMarketId",
                table: "Projects",
                column: "CommissioningMarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProductId",
                table: "Projects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionManagedLists_ManagedListId",
                table: "QuestionManagedLists",
                column: "ManagedListId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionManagedLists_QuestionnaireLineId_ManagedListId",
                table: "QuestionManagedLists",
                columns: new[] { "QuestionnaireLineId", "ManagedListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId",
                table: "QuestionnaireLines",
                columns: new[] { "ProjectId", "QuestionBankItemId" },
                unique: true,
                filter: "\"QuestionBankItemId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireLines_QuestionBankItemId",
                table: "QuestionnaireLines",
                column: "QuestionBankItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSubsetLinks_ManagedListId",
                table: "QuestionSubsetLinks",
                column: "ManagedListId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSubsetLinks_QuestionnaireLineId_ManagedListId",
                table: "QuestionSubsetLinks",
                columns: new[] { "QuestionnaireLineId", "ManagedListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSubsetLinks_SubsetDefinitionId",
                table: "QuestionSubsetLinks",
                column: "SubsetDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_FieldworkMarketId",
                table: "Studies",
                column: "FieldworkMarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_MasterStudyId",
                table: "Studies",
                column: "MasterStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_ParentStudyId",
                table: "Studies",
                column: "ParentStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_ProjectId_MasterStudyId_Version",
                table: "Studies",
                columns: new[] { "ProjectId", "MasterStudyId", "Version" },
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

            migrationBuilder.CreateIndex(
                name: "IX_SubsetDefinitions_ManagedListId",
                table: "SubsetDefinitions",
                column: "ManagedListId");

            migrationBuilder.CreateIndex(
                name: "IX_SubsetDefinitions_ProjectId_ManagedListId",
                table: "SubsetDefinitions",
                columns: new[] { "ProjectId", "ManagedListId" });

            migrationBuilder.CreateIndex(
                name: "IX_SubsetDefinitions_ProjectId_ManagedListId_SignatureHash",
                table: "SubsetDefinitions",
                columns: new[] { "ProjectId", "ManagedListId", "SignatureHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubsetMemberships_ManagedListItemId",
                table: "SubsetMemberships",
                column: "ManagedListItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SubsetMemberships_SubsetDefinitionId_ManagedListItemId",
                table: "SubsetMemberships",
                columns: new[] { "SubsetDefinitionId", "ManagedListItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionManagedLists");

            migrationBuilder.DropTable(
                name: "QuestionSubsetLinks");

            migrationBuilder.DropTable(
                name: "StudyManagedListAssignments");

            migrationBuilder.DropTable(
                name: "StudyQuestionSubsetLinks");

            migrationBuilder.DropTable(
                name: "SubsetMemberships");

            migrationBuilder.DropTable(
                name: "QuestionnaireLines");

            migrationBuilder.DropTable(
                name: "StudyQuestionnaireLines");

            migrationBuilder.DropTable(
                name: "ManagedListItems");

            migrationBuilder.DropTable(
                name: "SubsetDefinitions");

            migrationBuilder.DropTable(
                name: "Studies");

            migrationBuilder.DropTable(
                name: "ManagedLists");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
