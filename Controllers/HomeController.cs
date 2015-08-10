using System.Web.Mvc;

namespace TemGente.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Privacidade()
        {
            return View();
        }
    }
}
