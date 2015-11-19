using Microsoft.Owin;
using Owin;
using TakeASeatApp;

[assembly: OwinStartup(typeof(Startup))]
namespace TakeASeatApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
