using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence.Configurations;

public class UsageRecordConfiguration
    : IEntityTypeConfiguration<UsageRecord>
{
    public void Configure(
        EntityTypeBuilder<UsageRecord> builder)
    {
        builder.ToTable("usage_records");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.EstimatedCost)
            .HasPrecision(18, 10);


        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CreatedAt
        });


        builder.HasOne(x => x.User)
            .WithMany(x => x.UsageRecords)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(x => x.Model)
            .WithMany(x => x.UsageRecords)
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasOne(x => x.Optimization)
            .WithMany()
            .HasForeignKey(x => x.OptimizationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}