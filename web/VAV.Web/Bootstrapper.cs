using System.Web.Mvc;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Unity.Mvc3;
using VAV.Web.Extensions;


namespace VAV.Web
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            ControllerBuilder.Current.SetControllerFactory(new ReportControllerFactory(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            var configuration = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            configuration.Configure(container, "VAVContainer");
            return container;
        }
    }
}