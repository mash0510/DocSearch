using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DocSearch.Startup))]
namespace DocSearch
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
