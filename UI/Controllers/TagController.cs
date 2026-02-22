using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class TagController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
