
namespace Rax.Providers.RaxSampleProviderApp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    // Specification for Rax

    // using Request = IDictionary<string, object>;
    // using Response = IDictionary<string, object>;
    // using Next = Action<Exception>;
    // using MiddleWare = Func<Request, Response, Next, Task>;
    // using Host = IDictionary<string, object>;
    // using HostRunner =  System.Action<Host, IEnumerable<Middleware>>;
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

    public class RaxSampleProviderApp : IEnumerable<RaxMiddleware>
    {
        private readonly IList<RaxMiddleware> _middlewares;
        private readonly IList<RaxMiddleware> _routes = new List<RaxMiddleware>();

        public RaxSampleProviderApp()
        {
            _middlewares = new List<RaxMiddleware>();
        }

        public RaxSampleProviderApp(IEnumerable<RaxMiddleware> middlewares)
        {
            if (middlewares == null)
                throw new ArgumentNullException("middlewares");

            _middlewares = middlewares.ToList();
        }

        public IEnumerator<RaxMiddleware> GetEnumerator()
        {
            return _middlewares.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static RaxRequest CreateRequest()
        {
            return new Request();
        }

        public static RaxResponse CreateResponse()
        {
            return new Response();
        }

        public Task Listen(RaxHost host)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            var tcs = new TaskCompletionSource<object>();

            object runner;
            if (host.TryGetValue("Start", out runner))
            {
                var hostRunner = runner as Func<IEnumerable<RaxMiddleware>, Task>;
                if (hostRunner == null)
                    throw new ApplicationException("Don't know how to run on the host (Invalid host.Start)");

                hostRunner(_middlewares)
                    .ContinueWith(t =>
                                  {
                                      if (t.IsFaulted) tcs.TrySetException(t.Exception);
                                      else if (t.IsCanceled) tcs.TrySetCanceled();
                                      else tcs.TrySetResult(null);
                                  });
            }
            else
            {
                throw new ApplicationException("Don't know how to start the host (host.Start not found)");
            }

            return tcs.Task;
        }

        public void Use(RaxMiddleware middleware)
        {
            _middlewares.Add(middleware);
        }

        public Task Router(RaxRequest request, RaxResponse response, RaxNext next)
        {
            var tcs = new TaskCompletionSource<object>();

            var enumerator = _routes.GetEnumerator();

            Action<Exception> innerNext = null;
            innerNext = exception =>
                   {
                       if (exception != null)
                       {
                           next(exception);
                           tcs.TrySetResult(null);
                           return;
                       }

                       if (enumerator.MoveNext())
                       {
                           enumerator.Current(request, response, innerNext)
                               .ContinueWith(t =>
                                             {
                                                 if (t.IsFaulted) tcs.TrySetException(t.Exception);
                                                 else if (t.IsCanceled) tcs.TrySetCanceled();
                                                 else innerNext(null);
                                             });
                       }
                       else
                       {
                           next(null);
                           //tcs.TrySetException(new ApplicationException("Route not handled by router for " + request["Path"]));
                       }
                   };

            innerNext(null);
            return tcs.Task;
        }

        private void All(string method, string path, Func<Request, Response, RaxNext, Task> body)
        {
            if (body == null) throw new ArgumentNullException("body");

            _routes.Add((raxRequest, raxResponse, next) =>
                             {
                                 var tcs = new TaskCompletionSource<object>();

                                 try
                                 {
                                     var req = (Request)raxRequest;
                                     var res = (Response)raxResponse;

                                     if (req.Method == method && req.Path == path)
                                     {

                                         body(req, res, next)
                                             .ContinueWith(t =>
                                                           {
                                                               if (t.IsFaulted) tcs.TrySetException(t.Exception);
                                                               else if (t.IsCanceled) tcs.TrySetCanceled();
                                                               else tcs.TrySetResult(null);
                                                           });
                                     }
                                     else
                                     {
                                         next(null);
                                         tcs.TrySetResult(null);
                                     }
                                 }
                                 catch (Exception ex)
                                 {
                                     tcs.TrySetException(ex);
                                 }

                                 return tcs.Task;
                             });

        }

        private void All(string method, string path, Action<Request, Response, RaxNext> body)
        {
            if (body == null) throw new ArgumentNullException("body");

            Func<Request, Response, RaxNext, Task> asyncBody =
                (req, res, next) =>
                {
                    var tcs = new TaskCompletionSource<object>();
                    try
                    {
                        body(req, res, next);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }

                    return tcs.Task;
                };

            All(method, path, asyncBody);
        }

        public void Get(string path, Func<Request, Response, RaxNext, Task> body)
        {
            All("GET", path, body);
        }

        public void Get(string path, Action<Request, Response, RaxNext> body)
        {
            All("GET", path, body);
        }
    }

}
