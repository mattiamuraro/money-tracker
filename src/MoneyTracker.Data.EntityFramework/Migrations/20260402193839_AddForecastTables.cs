using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddForecastTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForecastRecurrenceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayInterval = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastRecurrenceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForecastExpenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecurrenceStart = table.Column<DateOnly>(type: "date", nullable: false),
                    RecurrenceEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    ForecastRecurrenceRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastExpenses_ForecastRecurrenceRules_ForecastRecurrenceRuleId",
                        column: x => x.ForecastRecurrenceRuleId,
                        principalTable: "ForecastRecurrenceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForecastIncomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecurrenceStart = table.Column<DateOnly>(type: "date", nullable: false),
                    RecurrenceEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    ForecastRecurrenceRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastIncomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastIncomes_ForecastRecurrenceRules_ForecastRecurrenceRuleId",
                        column: x => x.ForecastRecurrenceRuleId,
                        principalTable: "ForecastRecurrenceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastExpenses_ForecastRecurrenceRuleId",
                table: "ForecastExpenses",
                column: "ForecastRecurrenceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastIncomes_ForecastRecurrenceRuleId",
                table: "ForecastIncomes",
                column: "ForecastRecurrenceRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastExpenses");

            migrationBuilder.DropTable(
                name: "ForecastIncomes");

            migrationBuilder.DropTable(
                name: "ForecastRecurrenceRules");
        }
    }
}
