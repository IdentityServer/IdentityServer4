using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}