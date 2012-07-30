
namespace Rax
{
    // Specification for Rax

    // using Request = IDictionary<string, object>;
    // using Response = IDictionary<string, object>;
    // using Next = Action<Exception>;
    // using MiddleWare = Func<Request, Response, Next, Task>;
    // using Host = IDictionary<string, object>;
    // using HostRunner =  System.Action<Host, IEnumerable<Middleware>>;

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
        System.Collections.Generic.IDictionary<string, object> /* environment */,
        System.Collections.Generic.IEnumerable< /* middleware */
            System.Func<
                System.Collections.Generic.IDictionary<string, object> /* request */,
                System.Collections.Generic.IDictionary<string, object> /* response */,
                System.Action<System.Exception> /* next */,
                System.Threading.Tasks.Task>
        >
    >;
}
