namespace Rax.Hosts.AspNet
{
    // using Request = IDictionary<string, object>;
    // using Response = IDictionary<string, object>;
    // using Next = Action<Exception>;
    // using MiddleWare = Func<Request, Response, Next, Task>;
    // using Host = IDictionary<string, object>;
    // using HostRunner =  System.Action<Host, IEnumerable<Middleware>>;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Routing;
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

    public class RaxAspNetHost : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _host = new Dictionary<string, object>();

        private readonly Func<RaxRequest> _requestFactory;
        private readonly Func<RaxRequest> _responseFactory;
        private readonly RouteCollection _routes;
        private readonly string _path;

        public RaxAspNetHost(Func<RaxRequest> requestFactory, Func<RaxResponse> responseFactory, string path)
            : this(requestFactory, responseFactory, RouteTable.Routes, path)
        {
        }

        public RaxAspNetHost(Func<RaxRequest> requestFactory, Func<RaxResponse> responseFactory, RouteCollection routes, string path)
        {
            _requestFactory = requestFactory;
            _responseFactory = responseFactory;
            _routes = routes;
            _path = path;

            _host["Start"] = (Func<RaxHost, IEnumerable<RaxMiddleware>, Task>)Start;
        }

        public static Task Start(RaxHost host, IEnumerable<RaxMiddleware> app)
        {
            var aspnetHost = host as RaxAspNetHost;
            if (aspnetHost == null)
                throw new ArgumentException("Host must be of type " + typeof(RaxAspNetHost).FullName);

            var tcs = new TaskCompletionSource<object>();

            try
            {
                var path = aspnetHost._path ?? string.Empty;
                path += "{*pathInfo}";
                if (path.StartsWith("/")) path = path.Substring(1);
                var route = new Route(path, new RaxAspNetHttpHandlerRouteHandler(aspnetHost._requestFactory, aspnetHost._responseFactory, app));
                aspnetHost._routes.Add(route);
                tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }


        private class RaxAspNetHandler : IHttpAsyncHandler
        {
            private readonly Func<RaxRequest> _requestFactory;
            private readonly Func<RaxRequest> _responseFactory;
            private readonly IEnumerable<RaxMiddleware> _middlewares;

            public RaxAspNetHandler(Func<RaxRequest> requestFactory, Func<RaxRequest> responseFactory, IEnumerable<RaxMiddleware> middlewares)
            {
                _requestFactory = requestFactory;
                _responseFactory = responseFactory;
                _middlewares = middlewares;
            }

            private Task ProcessRequestAsync(HttpContext context)
            {
                var tcs = new TaskCompletionSource<object>();

                var request = _requestFactory();
                var response = _responseFactory();

                var httpRequest = context.Request;
                var httpResponse = context.Response;

                request["Method"] = httpRequest.HttpMethod;
                request["Path"] = httpRequest.Path;
                request["rax.ApplicationPath"] = context.Server.MapPath("~/");

                response["SetHeader"] = (Action<string, string[]>)
                ((key, value) =>
                 {
                     if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                     {
                         httpResponse.ContentType = string.Join(",", value);
                     }
                     else
                     {
                         tcs.TrySetException(new NotImplementedException("SetHeader does not implement setting " + key));
                     }
                 });
                response["Stream"] = httpResponse.OutputStream;
                response["End"] = (Action)(() =>
                                            {
                                                try
                                                {
                                                    httpResponse.End();
                                                    tcs.TrySetResult(null);
                                                }
                                                catch (Exception ex)
                                                {
                                                    tcs.TrySetResult(ex);
                                                }
                                            });

                var enumerator = _middlewares.GetEnumerator();

                Action<Exception> next = null;
                next = exception =>
                                          {
                                              if (exception != null)
                                              {
                                                  tcs.TrySetException(exception);
                                                  return;
                                              }

                                              if (enumerator.MoveNext())
                                              {
                                                  enumerator.Current(request, response, next)
                                                      .ContinueWith(t =>
                                                                    {
                                                                        if (t.IsFaulted) tcs.TrySetException(t.Exception);
                                                                        else if (t.IsCanceled) tcs.TrySetCanceled();
                                                                        else next(null);
                                                                    });
                                              }
                                              else
                                              {
                                                  tcs.TrySetException(new ApplicationException("Route not handled for " + request["Path"]));
                                              }
                                          };

                next(null);

                return tcs.Task;
            }

            private Task ProcessRequestAsync(HttpContext context, AsyncCallback cb)
            {
                return ProcessRequestAsync(context)
                    .ContinueWith(task => cb(task));
            }

            public void ProcessRequest(HttpContext context)
            {
                ProcessRequestAsync(context).Wait();
            }

            public bool IsReusable
            {
                get { return true; }
            }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
            {
                return ProcessRequestAsync(context, cb);
            }

            public void EndProcessRequest(IAsyncResult result)
            {
                if (result == null)
                    return;
                ((Task)result).Wait();
            }
        }

        private class RaxAspNetHttpHandlerRouteHandler : IRouteHandler
        {
            private readonly Func<RaxRequest> _requestFactory;
            private readonly Func<RaxRequest> _responseFactory;
            private readonly IEnumerable<RaxMiddleware> _middlewares;

            public RaxAspNetHttpHandlerRouteHandler(Func<RaxRequest> requestFactory, Func<RaxRequest> responseFactory, IEnumerable<RaxMiddleware> middlewares)
            {
                _requestFactory = requestFactory;
                _responseFactory = responseFactory;
                _middlewares = middlewares;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new RaxAspNetHandler(_requestFactory, _responseFactory, _middlewares);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _host.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _host.Add(item);
        }

        public void Clear()
        {
            _host.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _host.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _host.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _host.Remove(item);
        }

        public int Count
        {
            get { return _host.Count; }
        }

        public bool IsReadOnly
        {
            get { return _host.IsReadOnly; }
        }

        public bool ContainsKey(string key)
        {
            return _host.ContainsKey("key");
        }

        public void Add(string key, object value)
        {
            _host.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _host.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _host.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get { return _host[key]; }
            set { _host[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return _host.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _host.Values; }
        }
    }
}