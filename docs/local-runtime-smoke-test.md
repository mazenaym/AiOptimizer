# Local runtime setup and MVP smoke test

Run commands from the repository root using PowerShell 7 and the .NET 10 SDK. No live AI provider was exercised when preparing this guide. Use your own credentials and an available model.

## 1. PostgreSQL and private configuration

Start your PostgreSQL service or existing container. Create a database and a login with migration permissions. The API uses Npgsql; Redis is not required by the active registration. Never put credentials in tracked appsettings files.

The API project has a UserSecretsId. Development loads its user-secrets automatically. This is local development storage, not an encrypted production vault. Environment variables override JSON/user-secrets; use your deployment secret store in production.

```powershell
$apiProject = 'src/PromptOptimizer.Api'
$dbConnection = Read-Host 'PostgreSQL connection string (Host, Port, Database, Username, Password)' -MaskInput
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $dbConnection --project $apiProject
Remove-Variable dbConnection
$jwtKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set 'Jwt:Key' $jwtKey --project $apiProject
Remove-Variable jwtKey
dotnet user-secrets set 'Jwt:Issuer' 'PromptOptimizer.Local' --project $apiProject
dotnet user-secrets set 'Jwt:Audience' 'PromptOptimizer.Local.Client' --project $apiProject
dotnet user-secrets set 'Jwt:AccessTokenExpirationMinutes' '30' --project $apiProject
dotnet user-secrets set 'Jwt:RefreshTokenExpirationDays' '7' --project $apiProject
```

Equivalent environment keys: `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenExpirationMinutes`, `Jwt__RefreshTokenExpirationDays`. JWT key, issuer, audience and positive access-token lifetime are checked at startup. The JWT key must contain at least 32 UTF-8 bytes.

## 2. Provider and model catalog

| Provider code | Configuration key | Environment variable | Requirement/default |
|---|---|---|---|
| `Gemini` | `AI:Gemini:ApiKey` | `AI__Gemini__ApiKey` | Required when selected |
| `Gemini` | `AI:Gemini:BaseUrl` | `AI__Gemini__BaseUrl` | Optional; `https://generativelanguage.googleapis.com/v1beta` |
| `OpenRouter` | `AI:OpenRouter:ApiKey` | `AI__OpenRouter__ApiKey` | Required when selected |
| `OpenRouter` | `AI:OpenRouter:BaseUrl` | `AI__OpenRouter__BaseUrl` | Optional; `https://openrouter.ai/api/v1` |
| `Ollama` | `AI:Ollama:BaseUrl` | `AI__Ollama__BaseUrl` | Optional; `http://localhost:11434` |

Ollama has no API-key setting in the current implementation. Start its server and install the exact model you intend to use. For cloud providers, select an identifier your account can access. Base URLs must be absolute HTTP(S), without credentials, query strings or fragments. Keep the cloud API version path if overriding a base URL. Unused optional providers do not block startup; selecting an unconfigured provider fails explicitly.

```powershell
$providerCode = Read-Host 'Provider code: Gemini, OpenRouter, or Ollama'
if ($providerCode -in @('Gemini', 'OpenRouter')) {
    $providerKey = Read-Host 'Provider API key' -MaskInput
    dotnet user-secrets set "AI:${providerCode}:ApiKey" $providerKey --project $apiProject
    Remove-Variable providerKey
}
$modelIdentifier = Read-Host 'Exact model identifier available to this provider/account'
dotnet user-secrets set 'Seed:Model:Enabled' 'true' --project $apiProject
dotnet user-secrets set 'Seed:Model:ProviderCode' $providerCode --project $apiProject
dotnet user-secrets set 'Seed:Model:ProviderName' $providerCode --project $apiProject
dotnet user-secrets set 'Seed:Model:Identifier' $modelIdentifier --project $apiProject
dotnet user-secrets set 'Seed:Model:Name' $modelIdentifier --project $apiProject
```

Equivalent environment keys: `Seed__Model__Enabled`, `Seed__Model__ProviderCode`, `Seed__Model__ProviderName`, `Seed__Model__Identifier`, `Seed__Model__Name`.

The existing seeder runs only in Development/Testing. It adds categories and, when enabled, an active provider/model pair. Without opt-in, a fresh database has no guaranteed selectable model. Prices and capabilities remain unknown; estimated cost remains null until reliable prices are populated. Optimization never creates arbitrary models.

Seeding is additive: existing records are not reactivated or updated. Reuse the exact stored provider-code casing on subsequent runs (database lookup is case-sensitive; factory resolution is case-insensitive). Do not associate an existing model identifier with another provider. Production catalog provisioning is explicit; this seeder does not run there. The schema/seed supports usable records, but a live request is necessary to verify provider availability.

## 3. CORS

`Cors:AllowedOrigins` is an explicit allowlist. Development JSON includes `http://localhost:3000` and `http://localhost:5173`; Production has no default allowed origins. Set `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc., or the equivalent JSON array.

Origins must contain HTTP(S) scheme, host and optional port, without paths, wildcards or credentials. Allowed headers: `Authorization`, `Content-Type`. Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS. Cookie credentials are not enabled; clients send JWTs in the Authorization header. CORS does not replace authentication or restrict non-browser clients. A denied origin receives no `Access-Control-Allow-Origin` header.

Use the HTTPS API URL directly; redirected HTTP preflights can fail. .NET merges array indexes across configuration sources: when overriding Development origins, replace every existing index or edit the non-secret Development array.

## 4. Migrations and startup

The observed EF CLI 10.0.5 is machine-global, not repository-pinned. Align it to the project's runtime without changing EF packages:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.12
# If absent: dotnet tool install --global dotnet-ef --version 10.0.12
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project src/PromptOptimizer.Infrastructure --startup-project src/PromptOptimizer.Api --configuration Release
dotnet dev-certs https --trust
dotnet run --project src/PromptOptimizer.Api --configuration Release --launch-profile https
```

The API also migrates at startup. `PreserveUnknownOptimizationMetrics` remains responsible for nullable metrics; CORS needs no migration. A failed PostgreSQL connection prevents startup. Inspect local startup logs and PostgreSQL connection settings/permissions; do not share credential-bearing logs.

API: `https://localhost:7027`. Development Swagger: `https://localhost:7027/swagger`. Keep the API running; use a second terminal for requests.

## 5. Register, login, create, select model, optimize

These PowerShell requests can be reproduced in Swagger or Postman using the same routes and JSON fields. Passwords need at least eight characters, a letter and a digit.

```powershell
$baseUrl = 'https://localhost:7027'
$email = Read-Host 'New local test account email'
$password = Read-Host 'Account password' -MaskInput
Invoke-RestMethod "$baseUrl/api/health"

# Register (200); use a new email or skip for an existing account.
$registration = @{ email = $email; password = $password; firstName = 'Local'; lastName = 'Tester' } | ConvertTo-Json
$null = Invoke-RestMethod "$baseUrl/api/auth/register" -Method Post -ContentType 'application/json' -Body $registration

# Login (200); accessToken supplies the bearer credential.
$loginBody = @{ email = $email; password = $password } | ConvertTo-Json
$login = Invoke-RestMethod "$baseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body $loginBody
Remove-Variable password, registration, loginBody
$headers = @{ Authorization = "Bearer $($login.accessToken)" }
# Swagger's Authorize box expects: Bearer followed by your accessToken.

# Categories are optional; choose a returned ID or use null below.
$categories = Invoke-RestMethod "$baseUrl/api/categories" -Headers $headers
$categories

# Create Prompt (201).
$promptBody = @{
    originalContent = 'Explain dependency injection to a junior .NET developer.'
    title = 'Dependency injection explanation'
    categoryId = $null
    language = 'English'
} | ConvertTo-Json
$prompt = Invoke-RestMethod "$baseUrl/api/prompts" -Method Post -Headers $headers -ContentType 'application/json' -Body $promptBody

# List active models/providers (200). Empty means catalog setup is incomplete.
$models = @(Invoke-RestMethod "$baseUrl/api/models" -Headers $headers)
$models | Format-Table id, name, providerCode, modelIdentifier
$modelId = Read-Host 'Copy the ID of the model configured above'

# Optimize (200). Body contains no user/provider/prompt text.
$optimizeBody = @{ modelId = $modelId; customInstructions = 'Keep the explanation clear and concise.' } | ConvertTo-Json
$result = Invoke-RestMethod "$baseUrl/api/prompts/$($prompt.id)/optimize" -Method Post -Headers $headers -ContentType 'application/json' -Body $optimizeBody
$result | ConvertTo-Json -Depth 5
```

Inspect `optimizationId`, `promptId`, `modelIdentifier`, `providerCode`, `optimizedContent`, `processingTimeMs`. Prompt-size metrics remain null until estimation is implemented. Provider usage is separate: `providerInputTokens`, `providerOutputTokens`, `providerTotalTokens`; missing counts remain null. `estimatedCost` requires both counts and both prices. Success means optimization and usage were saved together. There is no history endpoint yet.

## 6. Runtime diagnosis

- Health 200/Healthy means API liveness, not dependency readiness. It does not call a provider or continuously probe PostgreSQL. A database outage after startup may leave health at 200 while database-backed requests fail.
- Startup failure: inspect PostgreSQL connectivity/migrations and required JWT settings. Invalid CORS origins also fail policy initialization.
- 401: missing/expired/invalid application JWT; log in again.
- 400: invalid request, empty model ID, instructions over 10,000 characters, or inactive/incomplete catalog metadata.
- 404: missing prompt/model or another user's prompt.
- 429: provider rate limit. The API also has a separate 60-requests/minute/IP limiter that may return an empty 429 body.
- 502: invalid/unusable provider response.
- 503: provider configuration, credentials, connectivity or timeout. The public response intentionally does not reveal which credential failed. Check the keys above, selected model and connectivity locally; do not enable secret-bearing HTTP logging.
- Browser CORS error: check the actual frontend origin and HTTPS API URL. This does not itself imply invalid authentication.

Automated tests do not establish live provider availability; they replace the engine and make no external AI calls.
