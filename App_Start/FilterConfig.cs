using System.Web.Mvc;
using TemGente.Controllers;

namespace TemGente
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorLoggerAttribute());
        }
    }
}