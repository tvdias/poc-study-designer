using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameProjectQuestionnairesToQuestionnaireLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ProjectQuestionnaires",
                newName: "QuestionnaireLines");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectQuestionnaires_ProjectId_QuestionBankItemId",
                table: "QuestionnaireLines",
                newName: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectQuestionnaires_QuestionBankItemId",
                table: "QuestionnaireLines",
                newName: "IX_QuestionnaireLines_QuestionBankItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "QuestionnaireLines",
                newName: "ProjectQuestionnaires");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId",
                table: "ProjectQuestionnaires",
                newName: "IX_ProjectQuestionnaires_ProjectId_QuestionBankItemId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionnaireLines_QuestionBankItemId",
                table: "ProjectQuestionnaires",
                newName: "IX_ProjectQuestionnaires_QuestionBankItemId");
        }
    }
}
