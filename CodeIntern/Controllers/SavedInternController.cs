using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class SavedInternController : Controller
    {
        public readonly IInternshipRepository _internshipRepository;
        private readonly ISavedInternRepository _savedInternRepo;
        private readonly UserManager<IdentityUser> _userManager;
        public SavedInternController(ISavedInternRepository db, UserManager<IdentityUser> userManager, IInternshipRepository internshipRepository)
        {
            _savedInternRepo = db;
            _userManager = userManager;
            _internshipRepository = internshipRepository;
        }
        public IActionResult Index()
        {
            //preko user id pretraziti po listi savedinternship pa pomocu toga pozvati internship i u listi displayat spremljene
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            List<SavedInternship> saved=_savedInternRepo.GetAll(x=>x.StudentId==userId).ToList();
            List<Internship> internships=new List<Internship>();
            Internship temp=new Internship();

            foreach(var item in saved)
            {
                temp = _internshipRepository.Get(x => x.InternshipId == item.InternshipId);
                internships.Add(temp);
            }



            return View(internships);
        }

        [HttpPost]
        public IActionResult Delete(int? id) 
        {
            SavedInternship? obj = _savedInternRepo.Get(x => x.InternshipId == id); ;
            if (obj == null)
            {
                return NotFound();
            }
            _savedInternRepo.Remove(obj);
            _savedInternRepo.Save();
            //TempData["success"] = "Internship deleted successfully";
            return RedirectToAction("Index");


        }

    }


}
