using PromptOptimizer.Domain.Common;
using PromptOptimizer.Domain.ValueObjects;

namespace PromptOptimizer.Domain.Entities;

public class UsageRecord : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? OptimizationId { get; set; }

    public Guid? ModelId { get; set; }


    // Tokens

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int TotalTokens { get; set; }


    // Cost

    public decimal EstimatedCost { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // Navigation

    public User User { get; set; } = null!;

    public Optimization? Optimization { get; set; }

    public AIModel? Model { get; set; }
}