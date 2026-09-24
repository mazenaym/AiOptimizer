using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence.Configurations;

public class PromptConfiguration
    : IEntityTypeConfiguration<Prompt>
{
    public void Configure(
        EntityTypeBuilder<Prompt> builder)
    {
        builder.ToTable("prompts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(255);

        builder.Property(x => x.OriginalContent)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(20);


        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.CreatedAt);


        builder.HasOne(x => x.User)
            .WithMany(x => x.Prompts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(x => x.Category)
            .WithMany(x => x.Prompts)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasMany(x => x.Optimizations)
            .WithOne(x => x.Prompt)
            .HasForeignKey(x => x.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}