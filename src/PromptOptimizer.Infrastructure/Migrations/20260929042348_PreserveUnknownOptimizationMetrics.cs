using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreserveUnknownOptimizationMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalTokens",
                table: "usage_records",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "OutputTokens",
                table: "usage_records",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "InputTokens",
                table: "usage_records",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "usage_records",
                type: "numeric(18,10)",
                precision: 18,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,10)",
                oldPrecision: 18,
                oldScale: 10);

            migrationBuilder.AlterColumn<int>(
                name: "TokensSaved",
                table: "optimizations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "OriginalTokens",
                table: "optimizations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "OptimizedTokens",
                table: "optimizations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        // Rollback requires resolving unknown values first; never replace them with fabricated zeroes.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalTokens",
                table: "usage_records",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OutputTokens",
                table: "usage_records",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InputTokens",
                table: "usage_records",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "usage_records",
                type: "numeric(18,10)",
                precision: 18,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,10)",
                oldPrecision: 18,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TokensSaved",
                table: "optimizations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OriginalTokens",
                table: "optimizations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OptimizedTokens",
                table: "optimizations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
