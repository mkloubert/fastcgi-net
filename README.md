# fastcgi-net

[FastCGI](http://www.fastcgi.com) library for .NET written in C#.

## Quick start

### Define settings

```csharp
var settings = new global::MarcelJoachimKloubert.FastCGI.Settings();

// is 9001 by default
settings.Port = 9002;
```

#### Set handler (HTTP)

```csharp
var httpHandler = new global::MarcelJoachimKloubert.FastCGI.Http.HttpRequestHandler();

// we are using an event to handle
// a HTTP request
httpHandler.Request += (sender, e) =>
    {
        // work with:
        
        // e.Request
        // e.Response
    };

settings.Handler = httpHandler;
```

### Start server

Create the instance from the `settings` variable:

```csharp
using (var server = new global::MarcelJoachimKloubert.FastCGI.Server(settings))
{
    server.Start();
    
    // press ENTER to dispose the server
    Console.ReadLine();
}
```

That's all!
