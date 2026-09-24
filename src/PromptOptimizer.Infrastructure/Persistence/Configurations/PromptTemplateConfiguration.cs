using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence.Configurations;

public class PromptTemplateConfiguration
    : IEntityTypeConfiguration<PromptTemplate>
{
    public void Configure(
        EntityTypeBuilder<PromptTemplate> builder)
    {
        builder.ToTable("prompt_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Template)
            .IsRequired();


        builder.Property(x => x.Variables)
            .HasColumnType("jsonb");


        builder.HasOne(x => x.Category)
            .WithMany(x => x.Templates)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}