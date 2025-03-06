# Asp Dot Net 8 Web API with Auth0 Integration

This is a sample project to demonstrate how to integrate Auth0 with Asp Dot Net Web API.

## configuration

### Install-Package

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### In the `appsettings.json` file, add the following code

```json
{
  "Auth0": {
    "Domain": "********.us.auth0.com",
    "Audience": "https://*********demo"
  }
}
```
> [!IMPORTANT]
> Replace the `Domain` and `Audience` with your Auth0 domain and audience.

### Create an `Extension folder` and add a class named `HasScopeRequirement.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Auth0_dot_net_8_api.Extensions;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Issuer { get; }
    public string Scope { get; }

    public HasScopeRequirement(string scope, string issuer)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
```

### In The same `Extension folder` and add a class named `ScopeHandler.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Auth0_dot_net_8_api.Extensions;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasScopeRequirement requirement
    ) {
        // If a user does not have the scope claim, get out of here
        if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        // Split the scope string into an array
        var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer)?.Value.Split(' ');

        // Succeed if the scope array contains the required scope
        if (scopes.Any(s => s == requirement.Scope))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

### In the `Program.cs` file, add the following code

```csharp
// Auth0 configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy(
            "admin:edit",
            policy => policy.Requirements.Add(
                new HasScopeRequirement("admin:edit", builder.Configuration["Auth0:Domain"]!)
            )
        );
    });
// Register the scope authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
```

### Add the following code to the after `app.UseHttpsRedirection();` in the `Program.cs` file

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### In the same `Program.cs` file, add the following code for configuring the Swagger UI to use the JWT token

```csharp
// swagger configuration for login
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth0-Dot-Net-8-Web-Api", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
```

> [!IMPORTANT]
> For configuring the api and user permissions, visit the below link for the above code to work properly.

### for more information, visit the below link
[https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/interactive](https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/interactive)


### In the `Controllers` folder, add below code to the `WeatherForecastController.cs` file to test if the authentication work

```csharp
[HttpGet("all")]
[Authorize]
public IActionResult Private()
{
     return Ok(new
     {
          Message = "Hello from a private endpoint!"
     });
}
    
[HttpGet("admin-only")]
[Authorize]
public IActionResult AdminOnly()
{
    return Ok(new
    {
        Message = "Admin only endpoint! and not edit privileges"
    });
}

[HttpGet("admin-private-scoped")]
// [Authorize("admin:edit")]
[Authorize]
public IActionResult Scoped()
{
     return Ok(new
     {
         Message = "Admin only endpoint! and with edit privileges"
     });
}
```

> [!IMPORTANT]
> go to your Auth0 account and go to your created API and add click the test and grab the JWT Barer token and use it in the swagger UI to test the above code/endpoints.

## Limitations
- [Authorize("admin:edit")] not working without a frontend. working on it.