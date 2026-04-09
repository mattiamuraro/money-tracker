using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReworkForecastRecurrenceRuleTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastRecurrenceRuleType",
                table: "ForecastRecurrenceRuleType");

            migrationBuilder.RenameTable(
                name: "ForecastRecurrenceRuleType",
                newName: "ForecastRecurrenceRuleTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastRecurrenceRuleTypes",
                table: "ForecastRecurrenceRuleTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRuleTypes_ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses",
                column: "ForecastRecurrenceRuleTypeId",
                principalTable: "ForecastRecurrenceRuleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRuleTypes_ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes",
                column: "ForecastRecurrenceRuleTypeId",
                principalTable: "ForecastRecurrenceRuleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_ForecastRecurrenceRuleTypes_ForecastRecurrenceRuleTypeId",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastIncomes_ForecastRecurrenceRuleTypes_ForecastRecurrenceRuleTypeId",
                table: "ForecastIncomes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForecastRecurrenceRuleTypes",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.RenameTable(
                name: "ForecastRecurrenceRuleTypes",
                newName: "ForecastRecurrenceRuleType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForecastRecurrenceRuleType",
                table: "ForecastRecurrenceRuleType",
                column: "Id");

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
    }
}
