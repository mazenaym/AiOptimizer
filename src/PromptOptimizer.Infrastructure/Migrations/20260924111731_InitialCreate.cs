using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MonthlyOptimizationLimit = table.Column<int>(type: "integer", nullable: true),
                    MonthlyTokenLimit = table.Column<long>(type: "bigint", nullable: true),
                    MaxSavedPrompts = table.Column<int>(type: "integer", nullable: true),
                    Features = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ai_models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ModelIdentifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InputPricePerMillionTokens = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    OutputPricePerMillionTokens = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    ContextWindow = table.Column<int>(type: "integer", nullable: true),
                    SupportsPromptCaching = table.Column<bool>(type: "boolean", nullable: false),
                    IsFree = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_models_ai_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "ai_providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prompt_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Template = table.Column<string>(type: "text", nullable: false),
                    Variables = table.Column<string>(type: "jsonb", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prompt_templates_PromptCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PromptCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OriginalContent = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsSaved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prompts_PromptCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PromptCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prompts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscriptions_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "optimizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    OptimizedContent = table.Column<string>(type: "text", nullable: false),
                    OriginalTokens = table.Column<int>(type: "integer", nullable: false),
                    OptimizedTokens = table.Column<int>(type: "integer", nullable: false),
                    TokensSaved = table.Column<int>(type: "integer", nullable: false),
                    ReductionPercentage = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    EstimatedOriginalCost = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: true),
                    EstimatedOptimizedCost = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: true),
                    EstimatedCostSaved = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: true),
                    OptimizationStrategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QualityScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_optimizations_ai_models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "ai_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_optimizations_prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usage_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptimizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    InputTokens = table.Column<int>(type: "integer", nullable: false),
                    OutputTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usage_records_ai_models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "ai_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usage_records_optimizations_OptimizationId",
                        column: x => x.OptimizationId,
                        principalTable: "optimizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usage_records_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_models_ModelIdentifier",
                table: "ai_models",
                column: "ModelIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_models_ProviderId",
                table: "ai_models",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_providers_Code",
                table: "ai_providers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_optimizations_CreatedAt",
                table: "optimizations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_optimizations_ModelId",
                table: "optimizations",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_optimizations_PromptId",
                table: "optimizations",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_plans_Code",
                table: "plans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prompt_templates_CategoryId",
                table: "prompt_templates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_CategoryId",
                table: "prompts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_CreatedAt",
                table: "prompts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_UserId",
                table: "prompts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_PlanId",
                table: "subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_IsActive",
                table: "subscriptions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_CreatedAt",
                table: "usage_records",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_ModelId",
                table: "usage_records",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_OptimizationId",
                table: "usage_records",
                column: "OptimizationId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_UserId",
                table: "usage_records",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_UserId_CreatedAt",
                table: "usage_records",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prompt_templates");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "usage_records");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "optimizations");

            migrationBuilder.DropTable(
                name: "ai_models");

            migrationBuilder.DropTable(
                name: "prompts");

            migrationBuilder.DropTable(
                name: "ai_providers");

            migrationBuilder.DropTable(
                name: "PromptCategories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
