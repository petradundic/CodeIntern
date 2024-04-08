using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace CodeIntern.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInternshipRepository _internshipRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<IdentityUser> _userManager;
        public HomeController(ILogger<HomeController> logger, IInternshipRepository internshipRepository, INotificationRepository notificationRepository, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _internshipRepository = internshipRepository;
            _notificationRepository = notificationRepository;
            _userManager = userManager;
        }

        public IActionResult Register()
        {
            return View();  
        }
        public IActionResult Index()
        {
            if(!_userManager.GetUserId(User).IsNullOrEmpty()) 
            {
                List<Notification> notificationList = _notificationRepository.GetAll(x => x.ToUser == _userManager.GetUserId(User) && x.IsRead==false).ToList();
                ViewBag.Notifications = notificationList;
            }
            

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
                //dodat results not found
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