using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence.Configurations;

public class OptimizationConfiguration
    : IEntityTypeConfiguration<Optimization>
{
    public void Configure(
        EntityTypeBuilder<Optimization> builder)
    {
        builder.ToTable("optimizations");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.OptimizedContent)
            .IsRequired();


        builder.Property(x => x.ReductionPercentage)
            .HasPrecision(7, 2);


        builder.Property(x => x.EstimatedOriginalCost)
            .HasPrecision(18, 10);

        builder.Property(x => x.EstimatedOptimizedCost)
            .HasPrecision(18, 10);

        builder.Property(x => x.EstimatedCostSaved)
            .HasPrecision(18, 10);


        builder.Property(x => x.QualityScore)
            .HasPrecision(5, 2);


        builder.Property(x => x.OptimizationStrategy)
            .HasMaxLength(100);


        builder.HasIndex(x => x.PromptId);

        builder.HasIndex(x => x.CreatedAt);


        builder.HasOne(x => x.Prompt)
            .WithMany(x => x.Optimizations)
            .HasForeignKey(x => x.PromptId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(x => x.Model)
            .WithMany(x => x.Optimizations)
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}