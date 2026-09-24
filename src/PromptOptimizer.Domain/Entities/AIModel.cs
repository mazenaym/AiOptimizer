using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public class AIModel : BaseEntity
    {
        public Guid ProviderId { get; set; }

        public string Name { get; set; } = null!;

        public string ModelIdentifier { get; set; } = null!;


        // Pricing

        public decimal? InputPricePerMillionTokens { get; set; }

        public decimal? OutputPricePerMillionTokens { get; set; }


        // Model capabilities

        public int? ContextWindow { get; set; }

        public bool SupportsPromptCaching { get; set; }

        public bool IsFree { get; set; }

        public bool IsActive { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation

        public AIProvider Provider { get; set; } = null!;

        public ICollection<Optimization> Optimizations { get; set; }
            = new List<Optimization>();

        public ICollection<UsageRecord> UsageRecords { get; set; }
            = new List<UsageRecord>();
    }
}
