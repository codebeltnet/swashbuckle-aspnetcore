---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions
example:
- *content
---

Extend XML documentation loading with various discovery strategies:

```csharp
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        
        // Load from a specific type's assembly
        documents.AddByType<Program>();
        
        // Load from a specific assembly
        documents.AddByAssembly(typeof(Program).Assembly);
        
        // Load from filename
        documents.AddByFilename("Documentation.xml");
        
        // Load from base directory
        documents.AddFromBaseDirectory<Program>();
        
        // Load from reference packs
        documents.AddFromReferencePacks<Program>();
    }
}
```

---
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddByType(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument},System.Type)
example:
- *content
---

Load XML documentation for a type's assembly:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddByType(typeof(Program));

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddByType`1(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument})
example:
- *content
---

Load XML documentation using generic type syntax:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddByType<Program>();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddByAssembly(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument},System.Reflection.Assembly)
example:
- *content
---

Load XML documentation for a specific assembly:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddByAssembly(typeof(Program).Assembly);

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddByFilename(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument},System.String)
example:
- *content
---

Load XML documentation from a specific filename:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddByFilename("Documentation.xml");

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddFromBaseDirectory(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument},System.Type)
example:
- *content
---

Load XML documentation from the base directory for a type's assembly:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddFromBaseDirectory(typeof(Program));

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddFromBaseDirectory`1(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument})
example:
- *content
---

Load XML documentation from the base directory using generic type syntax:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddFromBaseDirectory<Program>();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddFromReferencePacks(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument},System.Type)
example:
- *content
---

Load XML documentation from reference packs for a type's assembly:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddFromReferencePacks(typeof(Program));

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
uid: Codebelt.Extensions.Swashbuckle.AspNetCore.XPathDocumentExtensions.AddFromReferencePacks`1(System.Collections.Generic.ICollection{System.Xml.XPath.XPathDocument})
example:
- *content
---

Load XML documentation from reference packs using generic type syntax:

```csharp
using System;
using Codebelt.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MySwaggerExample;

public class Program
{
    public static void Main(string[] args)
    {
        var documents = new List<XPathDocument>();
        documents.AddFromReferencePacks<Program>();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRestfulSwagger(o =>
        {
            foreach (var doc in documents)
            {
                o.XmlDocumentations.Add(doc);
            }
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
