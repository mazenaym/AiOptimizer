using PromptOptimizer.Domain.Common;
using PromptOptimizer.Domain.Enums;

namespace PromptOptimizer.Domain.Entities;

public class Prompt : AuditableEntity
{
    public Guid UserId { get; set; }

    public Guid? CategoryId { get; set; }

    public string? Title { get; set; }

    public string OriginalContent { get; set; } = null!;

    public string? Language { get; set; }

    public bool IsSaved { get; set; }


    // Navigation

    public User User { get; set; } = null!;

    public PromptCategory? Category { get; set; }

    public ICollection<Optimization> Optimizations { get; set; }
        = new List<Optimization>();
}
