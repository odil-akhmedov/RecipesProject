using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RecipesProject.Startup))]
namespace RecipesProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
