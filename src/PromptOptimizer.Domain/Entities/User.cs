using PromptOptimizer.Domain.Common;
using PromptOptimizer.Domain.Enums;

namespace PromptOptimizer.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; }

    public DateTime? LastLoginAt { get; set; }


    // Navigation Properties

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    public ICollection<Prompt> Prompts { get; set; }
        = new List<Prompt>();

    public ICollection<UsageRecord> UsageRecords { get; set; }
        = new List<UsageRecord>();

    public ICollection<Subscription> Subscriptions { get; set; }
        = new List<Subscription>();
}
