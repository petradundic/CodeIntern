using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class InternshipApplicationController : Controller
    {
        public readonly IInternApplicationRepository _internApplicationRepo;
        public readonly IInternshipRepository _internshipRepository;
        public readonly UserManager<IdentityUser> _userManager;

        public InternshipApplicationController(IInternApplicationRepository db, IInternshipRepository internshipRepository, UserManager<IdentityUser> userManager)
        {
            _internApplicationRepo = db;
            _internshipRepository = internshipRepository;
            _userManager = userManager;
        }
        public IActionResult Index(int? internshipId)
        {
            List<InternshipApplication> applications = null;
            if(internshipId != null)
            {
                applications = _internApplicationRepo.GetAll(x=>x.InternshipId==internshipId).ToList();
            }
            else
            {
                applications = _internApplicationRepo.GetAll().ToList();
            }
            return View(applications);
        }
        public IActionResult Details(int id)
        {
             InternshipApplication?  InternshipApplicationFromDb =  _internApplicationRepo.Get(x => x. InternshipApplicationId == id);
            return View( InternshipApplicationFromDb);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(InternshipApplication obj, int InternshipId)
        {
            
                Internship internship = _internshipRepository.Get(x => x.InternshipId == InternshipId);
                var userId = _userManager.GetUserId(User);

                obj.InternshipId = InternshipId;
                obj.StudentId=userId;
                obj.DateCreated = DateTime.Now;
                obj.Status = "Applied";
                _internApplicationRepo.Add(obj);
                _internApplicationRepo.Save();
                internship.NumOfApplications += 1;
                _internshipRepository.Update(internship);
                _internshipRepository.Save();
           
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // InternshipApplication?  InternshipApplicationFromDb = _unitOfWork. InternshipApplication.Get(u => u.Id == id);
             InternshipApplication?  InternshipApplicationFromDb1 =  _internApplicationRepo.Get(u => u. InternshipApplicationId == id);
            // InternshipApplication?  InternshipApplicationFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

            if (InternshipApplicationFromDb1 == null)
            {
                return NotFound();
            }
            return View(InternshipApplicationFromDb1);
        }
        [HttpPost]
        public IActionResult Edit( InternshipApplication obj)
        {
            if (ModelState.IsValid)
            {
                 _internApplicationRepo.Update(obj);
                 _internApplicationRepo.Save();
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
             InternshipApplication?  InternshipApplicationFromDb =  _internApplicationRepo.Get(x => x. InternshipApplicationId == id);

            if ( InternshipApplicationFromDb == null)
            {
                return NotFound();
            }
            return View( InternshipApplicationFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
             InternshipApplication? obj =  _internApplicationRepo.Get(x => x. InternshipApplicationId == id);
            if (obj == null)
            {
                return NotFound();
            }
             _internApplicationRepo.Remove(obj);
             _internApplicationRepo.Save();
            TempData["success"] = " InternshipApplication deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
