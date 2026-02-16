using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Clients_ClientId1",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Products_ProductId1",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ClientId1",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProductId1",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "Projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId1",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId1",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId1",
                table: "Projects",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProductId1",
                table: "Projects",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Clients_ClientId1",
                table: "Projects",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Products_ProductId1",
                table: "Projects",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
