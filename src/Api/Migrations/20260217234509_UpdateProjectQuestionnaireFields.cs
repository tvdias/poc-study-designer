using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectQuestionnaireFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerMax",
                table: "ProjectQuestionnaires",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnswerMin",
                table: "ProjectQuestionnaires",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ColumnSortOrder",
                table: "ProjectQuestionnaires",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomNotes",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDummy",
                table: "ProjectQuestionnaires",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QuestionFormatDetails",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionRationale",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionText",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionTitle",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowSortOrder",
                table: "ProjectQuestionnaires",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScraperNotes",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariableName",
                table: "ProjectQuestionnaires",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ProjectQuestionnaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerMax",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "AnswerMin",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "ColumnSortOrder",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "CustomNotes",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "IsDummy",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "QuestionFormatDetails",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "QuestionRationale",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "QuestionText",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "QuestionTitle",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "RowSortOrder",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "ScraperNotes",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "VariableName",
                table: "ProjectQuestionnaires");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ProjectQuestionnaires");
        }
    }
}
