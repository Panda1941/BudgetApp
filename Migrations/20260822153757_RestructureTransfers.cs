using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetApp.Migrations
{
    /// <inheritdoc />
    public partial class RestructureTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialEvents_Accounts_DestinationAccountId",
                table: "FinancialEvents");

            migrationBuilder.DropIndex(
                name: "IX_FinancialEvents_DestinationAccountId",
                table: "FinancialEvents");

            migrationBuilder.DropColumn(
                name: "DestinationAccountId",
                table: "FinancialEvents");

            migrationBuilder.AddColumn<bool>(
                name: "IsTransfer",
                table: "FinancialEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferPairId",
                table: "FinancialEvents",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTransfer",
                table: "FinancialEvents");

            migrationBuilder.DropColumn(
                name: "TransferPairId",
                table: "FinancialEvents");

            migrationBuilder.AddColumn<int>(
                name: "DestinationAccountId",
                table: "FinancialEvents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialEvents_DestinationAccountId",
                table: "FinancialEvents",
                column: "DestinationAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialEvents_Accounts_DestinationAccountId",
                table: "FinancialEvents",
                column: "DestinationAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
