using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence.Configurations;

public class PlanConfiguration
    : IEntityTypeConfiguration<Plan>
{
    public void Configure(
        EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();


        builder.Property(x => x.MonthlyPrice)
            .HasPrecision(12, 2);


        builder.Property(x => x.Features)
            .HasColumnType("jsonb");


        builder.HasMany(x => x.Subscriptions)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}