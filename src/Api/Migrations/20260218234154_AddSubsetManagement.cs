using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsetManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "QuestionSubsetLinks");

            migrationBuilder.DropTable(
                name: "SubsetMemberships");

            migrationBuilder.DropTable(
                name: "SubsetDefinitions");
        }
    }
}
