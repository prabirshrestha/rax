# Rax
Web Interface for .NET

This project does not compete with OWIN but rather is a my playground to check if OWIN/rax features are enough to solve
the web interface problems in .net. More info can be found at http://owin.org/

There are some limitation in the current owin draft 7, so rax is was the result of it. 

**This is just a proof of concept and is not complete. Performance tuning has been left out for now.**

# Spec

```csharp
// note: prepend with Rax so it does not conflict when copy pasting coz most likely 
// Rax implementors will have objects called Request or Response.

// using Request = IDictionary<string, object>;
// using Response = IDictionary<string, object>;
// using Next = Action<Exception>;
// using MiddleWare = Func<Request, Response, Next, Task>;
// using Host = IDictionary<string, object>;
// using HostRunner =  System.Action<Host, IEnumerable<Middleware>>;

using RaxRequest = System.Collections.Generic.IDictionary<string, object>;
using RaxResponse = System.Collections.Generic.IDictionary<string, object>;
using RaxNext = System.Action<System.Exception>;

using RaxMiddleware = System.Func<
    System.Collections.Generic.IDictionary<string, object> /* request */,
    System.Collections.Generic.IDictionary<string, object> /* response */,
    System.Action<System.Exception> /* next */,
    System.Threading.Tasks.Task>;

using RaxHost = System.Collections.Generic.IDictionary<string, object>;

using RaxHostRunner = System.Action<
    System.Collections.Generic.IDictionary<string, object> /* host */,
    System.Collections.Generic.IEnumerable< /* middleware */
        System.Func<
            System.Collections.Generic.IDictionary<string, object> /* request */,
            System.Collections.Generic.IDictionary<string, object> /* response */,
            System.Action<System.Exception> /* next */,
            System.Threading.Tasks.Task>
    >
>;
```

How it looks in asp.net

```csharp
public class Global : System.Web.HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        var app = new RaxSampleProviderApp();

        app.Use(new Favicon("/public/favicon.ico").Middleware);
        app.Use(app.Router);

        app.Get("/", (req, res, next) =>
        {
            res.Write("<a href='/hi'>/hi</a><br/>");
            res.Write("<a href='/hello'>/hello</a><br/>");
            res.Write("<a href='/favicon.ico'>/favicon.ico (middleware)</a><br/>");
            res.End();
        });

        app.Get("/hi", (req, res, next) =>
        {
            res.Write("hi");
            res.End();
        });

        app.Get("/hello", (req, res, next) =>
        {
            res.Write("hello");
            res.End();
        });

        var host = new RaxAspNetHost(RaxSampleProviderApp.CreateRequest, RaxSampleProviderApp.CreateResponse, "/");
        RaxAspNetHost.Start(host, app).Wait();
    }
}
```

This of `RaxSampleProviderApp` as a web framework (Gate/NancyFx/Simple.Web/ASP.NET MVC)

`RaxAspNetHost = RaxHost` so you can change this to self host/wcf/asp.net/mono xps. (currently only asp.net host is provided)
`RaxSampleProviderApp = IEnumerable<RaxMiddleware>` this of this as web frameworks - Gate/NancyFx/Simple.Web/ASP.NET MVC
`new Favicon("/public/favicon.ico").Middleware = RaxMiddleware`
`app.Router = RaxMiddleware` provided by RaxSampleProviderApp

