using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Domain.Entities
{
    public class PromptCategory : BaseEntity
    {
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;


        // Navigation

        public ICollection<Prompt> Prompts { get; set; }
            = new List<Prompt>();

        public ICollection<PromptTemplate> Templates { get; set; }
            = new List<PromptTemplate>();
    }
}
