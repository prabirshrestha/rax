namespace Rax.Middlewares.Favicon
{
    // using Request = IDictionary<string, object>;
    // using Response = IDictionary<string, object>;
    // using Next = Action<Exception>;
    // using MiddleWare = Func<Request, Response, Next, Task>;
    // using Host = IDictionary<string, object>;
    // using HostRunner =  System.Action<Host, IEnumerable<Middleware>>;
    using System.IO;
    using System.Threading.Tasks;
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

    public class Favicon
    {
        private readonly string _path;

        public Favicon()
            : this(null)
        {
        }

        public Favicon(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "favicon.ico";
            if (path.StartsWith("/"))
                path = path.Substring(1);
            _path = path;
        }

        public Task Middleware(RaxRequest request, RaxResponse response, RaxNext next)
        {
            var tcs = new TaskCompletionSource<object>();

            if ((string)request["Path"] != "/favicon.ico")
            {
                next(null);
                tcs.TrySetResult(null);
                return tcs.Task;
            }

            var appPath = (string)request["rax.ApplicationPath"];

            var faviconPath = Path.Combine(appPath, _path);
            dynamic res = response;

            if (File.Exists(faviconPath))
            {
                var data = File.ReadAllBytes(faviconPath);
                res.SetHeader("Content-Type", "image/x-icon");
                res.Write(data, 0, data.Length);
                res.End();
            }
            else
            {
                res.End();
            }

            tcs.TrySetResult(null);

            return tcs.Task;
        }
    }
}