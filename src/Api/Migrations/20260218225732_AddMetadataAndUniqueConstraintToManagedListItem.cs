using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataAndUniqueConstraintToManagedListItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagedListItems_ManagedListId",
                table: "ManagedListItems");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "ManagedListItems",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagedListItems_ManagedListId_Value",
                table: "ManagedListItems",
                columns: new[] { "ManagedListId", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagedListItems_ManagedListId_Value",
                table: "ManagedListItems");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "ManagedListItems");

            migrationBuilder.CreateIndex(
                name: "IX_ManagedListItems_ManagedListId",
                table: "ManagedListItems",
                column: "ManagedListId");
        }
    }
}
