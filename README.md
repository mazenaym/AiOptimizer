"# AiOptimizer"  "# AiOptimizer" 
Auth 
Register
Register
   ↓
UserManager.CreateAsync()
   ↓
Identity hashes password
   ↓
GenerateAccessToken()
   ↓
IRefreshTokenService.CreateAsync()
   ↓
Save RefreshToken
   ↓
AuthResponse
Login
Login
   ↓
UserManager.FindByEmailAsync()
   ↓
SignInManager.CheckPasswordSignInAsync()
   ↓
GenerateAccessToken()
   ↓
IRefreshTokenService.CreateAsync()
   ↓
Save RefreshToken
   ↓
AuthResponse
والمسؤوليات:
UserManager
    → User management + Password

SignInManager
    → Login/password verification + lockout

IJwtService
    → Access Token

IRefreshTokenService
    → Refresh Token generation + persistence

AppDbContext
    → Infrastructure فقط 