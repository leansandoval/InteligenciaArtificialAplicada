using Microsoft.AspNetCore.Mvc;

namespace HolaMundoWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
