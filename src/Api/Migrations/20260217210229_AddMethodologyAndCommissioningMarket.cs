using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMethodologyAndCommissioningMarket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommissioningMarketId",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Methodology",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CommissioningMarketId",
                table: "Projects",
                column: "CommissioningMarketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_CommissioningMarkets_CommissioningMarketId",
                table: "Projects",
                column: "CommissioningMarketId",
                principalTable: "CommissioningMarkets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_CommissioningMarkets_CommissioningMarketId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CommissioningMarketId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CommissioningMarketId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Methodology",
                table: "Projects");
        }
    }
}
