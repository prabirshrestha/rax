
namespace Rax.Samples.RaxSampleProvidersApp.AspNetHost
{
    using System;
    using ActualRaxApp;
    using Rax.Hosts.AspNet;
    using Rax.Providers.RaxSampleProviderApp;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var app = new SampleApp().Setup();
            var host = new RaxAspNetHost(RaxSampleProviderApp.CreateRequest, RaxSampleProviderApp.CreateResponse, "/");
            RaxAspNetHost.Start(host, app).Wait();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}