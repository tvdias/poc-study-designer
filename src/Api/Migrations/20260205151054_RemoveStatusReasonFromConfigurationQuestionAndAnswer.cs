using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusReasonFromConfigurationQuestionAndAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "ConfigurationQuestions");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "ConfigurationAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "ConfigurationQuestions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "ConfigurationAnswers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
