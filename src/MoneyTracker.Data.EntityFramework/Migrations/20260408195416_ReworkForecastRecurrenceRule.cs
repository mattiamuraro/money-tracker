using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReworkForecastRecurrenceRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DayInterval",
                table: "ForecastRecurrenceRules",
                newName: "Interval");

            migrationBuilder.AddColumn<Guid>(
                name: "ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ForecastRecurrenceRuleType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastRecurrenceRuleType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRecurrenceRules_ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules",
                column: "ForecastRecurrenceRuleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastRecurrenceRules_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules",
                column: "ForecastRecurrenceRuleTypeId",
                principalTable: "ForecastRecurrenceRuleType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastRecurrenceRules_ForecastRecurrenceRuleType_ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules");

            migrationBuilder.DropTable(
                name: "ForecastRecurrenceRuleType");

            migrationBuilder.DropIndex(
                name: "IX_ForecastRecurrenceRules_ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules");

            migrationBuilder.DropColumn(
                name: "ForecastRecurrenceRuleTypeId",
                table: "ForecastRecurrenceRules");

            migrationBuilder.RenameColumn(
                name: "Interval",
                table: "ForecastRecurrenceRules",
                newName: "DayInterval");
        }
    }
}
