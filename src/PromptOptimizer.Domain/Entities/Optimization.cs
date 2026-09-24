using PromptOptimizer.Domain.Common;
using PromptOptimizer.Domain.Enums;
using PromptOptimizer.Domain.ValueObjects;

namespace PromptOptimizer.Domain.Entities;

public class Optimization : BaseEntity
{
    public Guid PromptId { get; set; }

    public Guid? ModelId { get; set; }

    public string OptimizedContent { get; set; } = null!;


    // Token information

    public int OriginalTokens { get; set; }

    public int OptimizedTokens { get; set; }

    public int TokensSaved { get; set; }

    public decimal? ReductionPercentage { get; set; }


    // Cost information

    public decimal? EstimatedOriginalCost { get; set; }

    public decimal? EstimatedOptimizedCost { get; set; }

    public decimal? EstimatedCostSaved { get; set; }


    // Optimization information

    public string? OptimizationStrategy { get; set; }

    public decimal? QualityScore { get; set; }

    public int? ProcessingTimeMs { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // Navigation

    public Prompt Prompt { get; set; } = null!;

    public AIModel? Model { get; set; }
}
