using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class CompanyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
