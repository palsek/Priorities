using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AspNetIdentityTry1.Startup))]

namespace AspNetIdentityTry1
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            app.UseCookieAuthentication(new Microsoft.Owin.Security.Cookies.CookieAuthenticationOptions() { 
                 AuthenticationType = "ApplicationCookie", 
                 LoginPath = new PathString("/auth/login")
            });
        }
    }
}
