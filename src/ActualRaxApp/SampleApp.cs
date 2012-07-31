
namespace ActualRaxApp
{
    using System.Collections.Generic;
    using Rax.Middlewares.Favicon;
    using Rax.Providers.RaxSampleProviderApp;
    using Request = System.Collections.Generic.IDictionary<string, object>;
    using Response = System.Collections.Generic.IDictionary<string, object>;
    using Next = System.Action<System.Exception>;

    using Middleware = System.Func<
        System.Collections.Generic.IDictionary<string, object> /* request */,
        System.Collections.Generic.IDictionary<string, object> /* response */,
        System.Action<System.Exception> /* next */,
        System.Threading.Tasks.Task>;

    using Host = System.Collections.Generic.IDictionary<string, object>;

    using HostRunner = System.Action<
        System.Collections.Generic.IDictionary<string, object> /* host */,
        System.Collections.Generic.IEnumerable< /* middleware */
            System.Func<
                System.Collections.Generic.IDictionary<string, object> /* request */,
                System.Collections.Generic.IDictionary<string, object> /* response */,
                System.Action<System.Exception> /* next */,
                System.Threading.Tasks.Task>
        >
    >;

    public class SampleApp
    {
        public IEnumerable<Middleware> Setup()
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

            return app;
        }
    }
}
