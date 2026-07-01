---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.SwaggerGenOptionsExtensions
example:
- *content
---

Add security schemes and user-agent documentation to Swagger API specifications. These extension methods are called within the `AddSwaggerGen(options => { ... })` setup action to document security mechanisms and required headers. The example shows how to compose multiple security scheme extensions together to document API Key, JWT Bearer, and Basic Authentication schemes, plus an additional user-agent header requirement. This declares the security options to the API consumer and generates the appropriate OpenAPI documentation:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            // Add security scheme documentation
            options.AddXApiKeySecurity();
            options.AddJwtBearerSecurity();
            options.AddBasicAuthenticationSecurity();
            
            // Add user-agent documentation
            options.AddUserAgent();
        });

        var app = builder.Build();
        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.SwaggerGenOptionsExtensions.AddUserAgent(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
example:
- *content
---

Document the user-agent header as a required parameter in all API operations. Call this extension method within the `AddSwaggerGen` setup action to automatically add user-agent documentation to every endpoint. When your API expects clients to identify themselves through a user-agent header, this extension ensures the requirement is clearly documented in the OpenAPI specification and displayed in the Swagger UI:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddUserAgent();
        });

        var app = builder.Build();
        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.SwaggerGenOptionsExtensions.AddXApiKeySecurity(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
example:
- *content
---

Document API Key authentication using the standard `X-API-Key` header. Call this extension method within `AddSwaggerGen` to declare API Key as a supported security scheme in the OpenAPI specification. The extension automatically creates a security scheme definition and a global security requirement, so all endpoints automatically inherit this authentication requirement and the Swagger UI shows the key header as required:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddXApiKeySecurity();
        });

        var app = builder.Build();
        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.SwaggerGenOptionsExtensions.AddJwtBearerSecurity(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
example:
- *content
---

Document OAuth 2.0 Bearer token security scheme:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddJwtBearerSecurity();
        });

        var app = builder.Build();
        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.SwaggerGenOptionsExtensions.AddBasicAuthenticationSecurity(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
example:
- *content
---

Document HTTP Basic authentication:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddBasicAuthenticationSecurity();
        });

        var app = builder.Build();
        if (string.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }
}
```
