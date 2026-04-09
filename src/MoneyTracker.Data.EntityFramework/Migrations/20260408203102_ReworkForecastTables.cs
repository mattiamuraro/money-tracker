using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReworkForecastTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRules_ForecastRecurrenceRuleId",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRules_ForecastRecurrenceRuleId",
                table: "ForecastIncomes");

            migrationBuilder.DropTable(
                name: "ForecastRecurrenceRules");

            migrationBuilder.RenameColumn(
                name: "ForecastRecurrenceRuleId",
                table: "ForecastIncomes",
                newName: "ForecastRecurrenceRuleTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ForecastIncomes_ForecastRecurrenceRuleId",
                table: "ForecastIncomes",
                newName: "IX_ForecastIncomes_ForecastRecurrenceRuleTypeId");

            migrationBuilder.RenameColumn(
                name: "ForecastRecurrenceRuleId",
                table: "ForecastExpenses",
                newName: "ForecastRecurrenceRuleTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ForecastExpenses_ForecastRecurrenceRuleId",
                table: "ForecastExpenses",
                newName: "IX_ForecastExpenses_ForecastRecurrenceRuleTypeId");

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "ForecastIncomes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "ForecastExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses",
                column: "ForecastRecurrenceRuleTypeId",
                principalTable: "ForecastRecurrenceRuleType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes",
                column: "ForecastRecurrenceRuleTypeId",
                principalTable: "ForecastRecurrenceRuleType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "ForecastExpenses");

            migrationBuilder.RenameColumn(
                name: "ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes",
                newName: "ForecastRecurrenceRuleId");

            migrationBuilder.RenameIndex(
                name: "IX_ForecastIncomes_ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes",
                newName: "IX_ForecastIncomes_ForecastRecurrenceRuleId");

            migrationBuilder.RenameColumn(
                name: "ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses",
                newName: "ForecastRecurrenceRuleId");

            migrationBuilder.RenameIndex(
                name: "IX_ForecastExpenses_ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses",
                newName: "IX_ForecastExpenses_ForecastRecurrenceRuleId");

            migrationBuilder.CreateTable(
                name: "ForecastRecurrenceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForecastRecurrenceRuleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastRecurrenceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastRecurrenceRules_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                        column: x => x.ForecastRecurrenceRuleTypeId,
                        principalTable: "ForecastRecurrenceRuleType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRecurrenceRules_ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules",
                column: "ForecastRecurrenceRuleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRules_ForecastRecurrenceRuleId",
                table: "ForecastExpenses",
                column: "ForecastRecurrenceRuleId",
                principalTable: "ForecastRecurrenceRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRules_ForecastRecurrenceRuleId",
                table: "ForecastIncomes",
                column: "ForecastRecurrenceRuleId",
                principalTable: "ForecastRecurrenceRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
