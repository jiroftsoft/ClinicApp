using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClinicApp.Startup))]
namespace ClinicApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
