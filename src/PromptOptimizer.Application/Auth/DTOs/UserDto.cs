using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Application.Auth.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}
