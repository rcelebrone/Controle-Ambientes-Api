using System.Web.Mvc;
using System.Web.Routing;

namespace TemGente
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Url da validação de acesso",
                url: "ValidarAcesso",
                defaults: new { controller = "Api", action = "ValidaHash", email = UrlParameter.Optional, hash = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}