using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid PlanId { get; set; }


        public DateTime StartedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation

        public User User { get; set; } = null!;

        public Plan Plan { get; set; } = null!;
    }
}
