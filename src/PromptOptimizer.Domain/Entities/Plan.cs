using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public class Plan : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public decimal MonthlyPrice { get; set; }


        // Limits

        public int? MonthlyOptimizationLimit { get; set; }

        public long? MonthlyTokenLimit { get; set; }

        public int? MaxSavedPrompts { get; set; }


        // Flexible features

        public string? Features { get; set; }


        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation

        public ICollection<Subscription> Subscriptions { get; set; }
            = new List<Subscription>();
    }
}
