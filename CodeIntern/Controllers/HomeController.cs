using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CodeIntern.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IInternshipRepository _internshipRepository;

        public HomeController(ILogger<HomeController> logger, IInternshipRepository internshipRepository)
        {
            _logger = logger;
            _internshipRepository = internshipRepository;
        }

        public IActionResult Register()
        {
            return View();  
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Search(string searchTerm) 
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                List<Internship> results= _internshipRepository.GetAll(x=> x.Title.Contains(searchTerm) || x.CompanyName==searchTerm || x.Description.Contains(searchTerm)).ToList();
                if(results.Count > 0)
                {
                    return RedirectToAction("Index", "Internship", results);
                }
            }

            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}