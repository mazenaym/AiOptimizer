using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using PromptOptimizer.Domain.Common;
using PromptOptimizer.Domain.Enums;

namespace PromptOptimizer.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();

    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}