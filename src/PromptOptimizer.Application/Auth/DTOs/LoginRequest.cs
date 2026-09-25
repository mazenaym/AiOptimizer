using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Application.Auth.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
