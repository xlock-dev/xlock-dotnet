# XLock.AspNetCore

ASP.NET Core middleware for [x-lock](https://x-lock.dev) bot protection.

## Install

```bash
dotnet add package XLock.AspNetCore --version 0.1.0
```

## Setup

**Program.cs**

```csharp
using XLock.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddXLock(options =>
{
    options.SiteKey = builder.Configuration["XLock:SiteKey"]!;
    options.ProtectedPaths = ["/api/auth", "/api/checkout"];
    // options.FailOpen = true;   // default — allow traffic if API is unreachable
});

var app = builder.Build();

app.UseXLock();

app.MapControllers();
app.Run();
```

**appsettings.json**

```json
{
  "XLock": {
    "SiteKey": "sk_..."
  }
}
```

## How it works

The middleware intercepts POST requests to protected paths and checks for an `x-lock` header token. Tokens are verified against the x-lock API:

- **v3 tokens** (`v3.<sessionId>.<rest>`) are routed to `/v3/session/enforce`
- **v1 tokens** are routed to `/v1/enforce`

If the token is missing or the API returns 403, the request is blocked. When `FailOpen` is true (default), requests are allowed through if the API is unreachable or returns an unexpected error.

## License

MIT
