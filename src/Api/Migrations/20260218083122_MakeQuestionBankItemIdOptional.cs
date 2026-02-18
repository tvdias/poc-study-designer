using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeQuestionBankItemIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing unique index
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId",
                table: "QuestionnaireLines");

            // Drop the foreign key temporarily
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireLines_QuestionBankItems_QuestionBankItemId",
                table: "QuestionnaireLines");

            // Alter the column to be nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionBankItemId",
                table: "QuestionnaireLines",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Recreate the foreign key with nullable constraint
            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireLines_QuestionBankItems_QuestionBankItemId",
                table: "QuestionnaireLines",
                column: "QuestionBankItemId",
                principalTable: "QuestionBankItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Recreate the unique index with filter for non-null values
            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId",
                table: "QuestionnaireLines",
                columns: new[] { "ProjectId", "QuestionBankItemId" },
                unique: true,
                filter: "[QuestionBankItemId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the filtered unique index
            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId",
                table: "QuestionnaireLines");

            // Drop the foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireLines_QuestionBankItems_QuestionBankItemId",
                table: "QuestionnaireLines");

            // Alter the column back to non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionBankItemId",
                table: "QuestionnaireLines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Recreate the foreign key with non-nullable constraint
            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireLines_QuestionBankItems_QuestionBankItemId",
                table: "QuestionnaireLines",
                column: "QuestionBankItemId",
                principalTable: "QuestionBankItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Recreate the unique index without filter
            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireLines_ProjectId_QuestionBankItemId",
                table: "QuestionnaireLines",
                columns: new[] { "ProjectId", "QuestionBankItemId" },
                unique: true);
        }
    }
}
