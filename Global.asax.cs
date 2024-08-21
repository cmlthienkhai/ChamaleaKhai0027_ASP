using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ChamaleaKhai0027
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            AreaRegistration.RegisterAllAreas(); // Đăng ký tất cả các khu vực
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
