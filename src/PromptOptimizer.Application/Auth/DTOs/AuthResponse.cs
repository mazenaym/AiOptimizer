using System;
using System.Collections.Generic;
using System.Text;

namespace PromptOptimizer.Application.Auth.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime AccessTokenExpiresAt { get; set; }

        public UserDto User { get; set; } = null!;
    }
}
