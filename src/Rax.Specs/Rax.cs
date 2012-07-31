
namespace Rax
{
    // Specification for Rax

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
}
