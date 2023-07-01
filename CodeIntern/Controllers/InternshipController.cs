using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace CodeIntern.Controllers
{
    public class InternshipController : Controller
    {
        IInternshipRepository _internshipRepo;
        private readonly ISavedInternRepository _savedInternRepo;
        private readonly UserManager<IdentityUser> _userManager;
        public InternshipController(UserManager<IdentityUser> userManager, ISavedInternRepository savedInternRepo, IInternshipRepository internshipRepository)
        {
            _userManager = userManager;
            _savedInternRepo = savedInternRepo;
            _internshipRepo = internshipRepository;
        }
        public IActionResult Index(string? companyId)
        {
            List<Internship> InternshipsList = _internshipRepo.GetAll().ToList();
            return View(InternshipsList);
        }
        public IActionResult Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            Internship? InternshipFromDb = _internshipRepo.Get(x => x.InternshipId == id);
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
            _internshipRepo.Add(obj);
            _internshipRepo.Save();
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Internship? InternshipFromDb = _unitOfWork.Internship.Get(u => u.Id == id);
            Internship? InternshipFromDb1 = _internshipRepo.Get(x => x.InternshipId == id); ;
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
                _internshipRepo.Update(obj);
                _internshipRepo.Save();
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
            Internship? InternshipFromDb = _internshipRepo.Get(x => x.InternshipId == id);

            if (InternshipFromDb == null)
            {
                return NotFound();
            }
            return View(InternshipFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Internship? obj = _internshipRepo.Get(x => x.InternshipId == id); ;
            if (obj == null)
            {
                return NotFound();
            }
            _internshipRepo.Remove(obj);
            _internshipRepo.Save();
            TempData["success"] = "Internship deleted successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Filter(bool? paid, string? location, string? position, string? technology, string? language, string? workPlace)
        {
            List<Internship> internships = new List<Internship>();

            if (paid.HasValue)
            {
                internships = _internshipRepo.GetAll(x => x.IsPaid == paid.Value).ToList();
            }
            else
            {
                internships = _internshipRepo.GetAll().ToList();
            }

            if (!string.IsNullOrEmpty(location))
            {
                internships = internships.Where(x => x.Location == location).ToList();
            }

            if (!string.IsNullOrEmpty(position))
            {
                internships = internships.Where(x => x.Position == position).ToList();
            }

            if (!string.IsNullOrEmpty(technology))
            {
                internships = internships.Where(x => x.Technology == technology).ToList();
            }

            if (!string.IsNullOrEmpty(language))
            {
                internships = internships.Where(x => x.ProgLanguage == language).ToList();
            }

            if (!string.IsNullOrEmpty(workPlace))
            {
                internships = internships.Where(x => x.WorkPlace == workPlace).ToList();
            }

            return RedirectToAction("Index",  internships );
        }

       
    }
}
