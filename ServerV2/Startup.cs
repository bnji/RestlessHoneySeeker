using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServerV2.Startup))]
namespace ServerV2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
