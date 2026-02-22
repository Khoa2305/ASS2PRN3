using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
