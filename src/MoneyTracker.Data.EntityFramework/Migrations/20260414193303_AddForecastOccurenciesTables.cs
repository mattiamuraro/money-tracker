using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddForecastOccurenciesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ForecastOccurrenceId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForecastOccurrenceId",
                table: "Incomes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ForecastIncomes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ForecastExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentCategoryId",
                table: "ForecastExpenses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ForecastOccurrenceStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001")),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001")),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastOccurrenceStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastOccurrenceStatuses_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastOccurrenceStatuses_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForecastOccurrences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForecastDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsIncome = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ForecastOccurrenceStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000101")),
                    ValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001")),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastOccurrences_ForecastOccurrenceStatuses_ForecastOccurrenceStatusId",
                        column: x => x.ForecastOccurrenceStatusId,
                        principalTable: "ForecastOccurrenceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastOccurrences_PaymentCategories_PaymentCategoryId",
                        column: x => x.PaymentCategoryId,
                        principalTable: "PaymentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastOccurrences_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastOccurrences_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ForecastOccurrenceId",
                table: "Payments",
                column: "ForecastOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_ForecastOccurrenceId",
                table: "Incomes",
                column: "ForecastOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRecurrenceRuleTypes_Code",
                table: "ForecastRecurrenceRuleTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastExpenses_PaymentCategoryId",
                table: "ForecastExpenses",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_CreatedById",
                table: "ForecastOccurrences",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_ExpectedDate",
                table: "ForecastOccurrences",
                column: "ExpectedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_ForecastDefinitionId",
                table: "ForecastOccurrences",
                column: "ForecastDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_ForecastOccurrenceStatusId",
                table: "ForecastOccurrences",
                column: "ForecastOccurrenceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_ModifiedById",
                table: "ForecastOccurrences",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_PaymentCategoryId",
                table: "ForecastOccurrences",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrenceStatuses_Code",
                table: "ForecastOccurrenceStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrenceStatuses_CreatedById",
                table: "ForecastOccurrenceStatuses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrenceStatuses_ModifiedById",
                table: "ForecastOccurrenceStatuses",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastExpenses_PaymentCategories_PaymentCategoryId",
                table: "ForecastExpenses",
                column: "PaymentCategoryId",
                principalTable: "PaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incomes_ForecastOccurrences_ForecastOccurrenceId",
                table: "Incomes",
                column: "ForecastOccurrenceId",
                principalTable: "ForecastOccurrences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ForecastOccurrences_ForecastOccurrenceId",
                table: "Payments",
                column: "ForecastOccurrenceId",
                principalTable: "ForecastOccurrences",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_PaymentCategories_PaymentCategoryId",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Incomes_ForecastOccurrences_ForecastOccurrenceId",
                table: "Incomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ForecastOccurrences_ForecastOccurrenceId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "ForecastOccurrences");

            migrationBuilder.DropTable(
                name: "ForecastOccurrenceStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ForecastOccurrenceId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_ForecastOccurrenceId",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastRecurrenceRuleTypes_Code",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastExpenses_PaymentCategoryId",
                table: "ForecastExpenses");

            migrationBuilder.DropColumn(
                name: "ForecastOccurrenceId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ForecastOccurrenceId",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ForecastExpenses");

            migrationBuilder.DropColumn(
                name: "PaymentCategoryId",
                table: "ForecastExpenses");
        }
    }
}
