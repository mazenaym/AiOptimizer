using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Application.Auth.DTOs
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
