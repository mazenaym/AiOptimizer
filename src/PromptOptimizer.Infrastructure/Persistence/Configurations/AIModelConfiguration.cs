using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence.Configurations;

public class AIModelConfiguration
    : IEntityTypeConfiguration<AIModel>
{
    public void Configure(
        EntityTypeBuilder<AIModel> builder)
    {
        builder.ToTable("ai_models");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ModelIdentifier)
            .HasMaxLength(255)
            .IsRequired();


        builder.Property(x => x.InputPricePerMillionTokens)
            .HasPrecision(18, 8);

        builder.Property(x => x.OutputPricePerMillionTokens)
            .HasPrecision(18, 8);


        builder.HasIndex(x => x.ModelIdentifier)
            .IsUnique();


        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Models)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}