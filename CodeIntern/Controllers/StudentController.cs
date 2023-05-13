using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
