using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CodeIntern.Controllers
{
    public class SavedInternController : Controller
    {
        private readonly IInternshipRepository _internshipRepository;
        private readonly ISavedInternRepository _savedInternRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        public SavedInternController(ISavedInternRepository db, UserManager<ApplicationUser> userManager, IInternshipRepository internshipRepository)
        {
            _savedInternRepo = db;
            _userManager = userManager;
            _internshipRepository = internshipRepository;
        }
        [Authorize(Roles = "Admin,Student")]
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

        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> Delete(int? id)
        {
            SavedInternship? obj = await _savedInternRepo.GetAsync(x => x.InternshipId == id);

            if (obj == null)
            {
                return NotFound();
            }

            _savedInternRepo.Remove(obj);
            await _savedInternRepo.SaveAsync();

            return RedirectToAction("Index");
        } 

    }


}
