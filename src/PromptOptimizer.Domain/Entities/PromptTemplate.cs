using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public class PromptTemplate : BaseEntity
    {
        public Guid? CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string Template { get; set; } = null!;

        public string? Variables { get; set; }

        public bool IsPublic { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation

        public PromptCategory? Category { get; set; }
    }
}
