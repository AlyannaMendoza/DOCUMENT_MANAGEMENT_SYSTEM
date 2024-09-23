using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DMS_v2.Startup))]
namespace DMS_v2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
