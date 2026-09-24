using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public class AIProvider : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation

        public ICollection<AIModel> Models { get; set; }
            = new List<AIModel>();
    }
}
