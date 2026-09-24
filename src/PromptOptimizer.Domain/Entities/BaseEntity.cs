using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
