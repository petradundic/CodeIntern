using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class InternshipController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISavedInternRepository _savedInternRepo;
        private readonly UserManager<IdentityUser> _userManager;
        public InternshipController(ApplicationDbContext db, UserManager<IdentityUser> userManager, ISavedInternRepository savedInternRepo)
        {
            _db = db;
            _userManager = userManager;
            _savedInternRepo = savedInternRepo;
        }
        public IActionResult Index(string? companyId)
        {
            List<Internship> InternshipsList = _db.Internship.ToList();
            return View(InternshipsList);
        }
        public IActionResult Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            Internship? InternshipFromDb = _db.Internship.Find(id);
            return View(InternshipFromDb);
        }
        public IActionResult SaveInternship(int id)
        {
            var userId = _userManager.GetUserId(User);
            SavedInternship savedInternship = new SavedInternship();
            savedInternship.InternshipId = id;
            savedInternship.DateSaved = DateTime.Now;
            if (userId != null)
            {
                savedInternship.StudentId = userId;
                _savedInternRepo.Add(savedInternship);
                _savedInternRepo.Save();
            }


            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Internship obj)
        {
            var userId = _userManager.GetUserId(User);
            obj.CompanyId = userId;
            obj.CreatedDate = DateTime.Now;
            _db.Internship.Add(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Internship? InternshipFromDb = _unitOfWork.Internship.Get(u => u.Id == id);
            Internship? InternshipFromDb1 = _db.Internship.FirstOrDefault(u => u.InternshipId == id);
            //Internship? InternshipFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

            if (InternshipFromDb1 == null)
            {
                return NotFound();
            }
            return View(InternshipFromDb1);
        }
        [HttpPost]
        public IActionResult Edit(Internship obj)
        {
            if (ModelState.IsValid)
            {
                _db.Internship.Update(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Internship? InternshipFromDb = _db.Internship.Find(id);

            if (InternshipFromDb == null)
            {
                return NotFound();
            }
            return View(InternshipFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Internship? obj = _db.Internship.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Internship.Remove(obj);
            _db.SaveChanges(true);
            TempData["success"] = "Internship deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
