using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
