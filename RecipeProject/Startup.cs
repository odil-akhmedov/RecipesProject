using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RecipeProject.Startup))]
namespace RecipeProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
