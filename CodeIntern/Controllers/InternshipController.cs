using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CodeIntern.Controllers
{
    public class InternshipController : Controller
    {
        private readonly IInternshipRepository _internshipRepo;
        private readonly ISavedInternRepository _savedInternRepo;
        private readonly IInternApplicationRepository _internApplicationRepo;
        private INotificationRepository _notificationRepository;
        private readonly UserManager<IdentityUser> _userManager;
        public InternshipController(UserManager<IdentityUser> userManager, ISavedInternRepository savedInternRepo, IInternshipRepository internshipRepository, IInternApplicationRepository internApplicationRepo, INotificationRepository notificationRepository)
        {
            _userManager = userManager;
            _savedInternRepo = savedInternRepo;
            _internshipRepo = internshipRepository;
            _internApplicationRepo = internApplicationRepo;
            _notificationRepository = notificationRepository;
        }
        public IActionResult Index(string? companyId, List<Internship>? obj)
        {
            List<Internship> internshipsList = _internshipRepo.GetAll().ToList();
            //List<string> tempLocations = new List<string>();

            //foreach (var intern in internshipsList)
            //{
            //    foreach (var loc in intern.Location)
            //    {
            //        tempLocations.Add(loc);
            //    }
            //}

            //IEnumerable<SelectListItem> locations = tempLocations
            //.Distinct()
            //.Select(loc => new SelectListItem { Text = loc, Value = loc })
            //.OrderBy(loc => loc.Text);
            IEnumerable<SelectListItem> locations = _internshipRepo.GetAll()
                .Where(x => x.Location != null)
                .Select(x => new SelectListItem { Text = x.Location, Value = x.Location })
                .DistinctBy(x => x.Text)
                .OrderBy(loc => loc.Text);

            IEnumerable<SelectListItem> technologies = _internshipRepo.GetAll()
            .Where(x => x.Technology != null)
            .Select(x => new SelectListItem { Text = x.Technology, Value = x.Technology })
            .DistinctBy(x => x.Text)
             .OrderBy(tech => tech.Text);

            IEnumerable<SelectListItem> positions = _internshipRepo.GetAll()
                .Where(x => x.Position != null)
                .Select(x => new SelectListItem { Text = x.Position, Value = x.Position })
                .DistinctBy(x => x.Text)
                .OrderBy(pos => pos.Text);

            IEnumerable<SelectListItem> progLanguages = _internshipRepo.GetAll()
                .Where(x => x.ProgLanguage != null)
                .Select(x => new SelectListItem { Text = x.ProgLanguage, Value = x.ProgLanguage })
                .DistinctBy(x => x.Text)
                .OrderBy(lang => lang.Text);

            ViewBag.Technologies = technologies;
            ViewBag.Positions = positions;
            ViewBag.ProgramLanguages = progLanguages;
            ViewBag.Locations = locations;

            if (obj != null && obj.Count > 0)
            {
                return View(obj);
            }

            internshipsList = _internshipRepo.GetAll(x => x.StartDate >= DateTime.Now).ToList();

            if (!string.IsNullOrEmpty(companyId))
            {
                internshipsList = _internshipRepo.GetAll(x => x.CompanyId == companyId).ToList();
            }
            return View(internshipsList);
        }

        [Authorize(Roles = "Company")]
        public IActionResult ExpiredInternships(List<Internship>? internships)
        {
            var userId = _userManager.GetUserId(User);

            List<Internship> InternshipsList = _internshipRepo.GetAll(x => x.StartDate < DateTime.Now && x.CompanyId == userId).ToList();
            if (InternshipsList != null && InternshipsList.Count > 0)
            {
                return View(InternshipsList);
            }
            //dodaj neku poruku
            return View("Index", userId);
        }
        public IActionResult Details(int id)
        {
            Internship? internshipFromDb = _internshipRepo.Get(x => x.InternshipId == id);
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;

            InternshipApplication? internApp = _internApplicationRepo.Get(x => x.InternshipId == id && x.StudentId == userId);
            SavedInternship? savedInternship = _savedInternRepo.Get(x => x.InternshipId == id && x.StudentId == userId);
            ViewBag.HasApplied = internApp != null;
            ViewBag.InternshipApplicationId = internApp?.InternshipApplicationId;
            ViewBag.IsSaved = savedInternship != null;


            return View(internshipFromDb);
        }

        [Authorize(Roles = "Student")]
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
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Create(Internship obj)
        {
            //dodaj uvjet i validaciju za start date da ne bude ranije od kreiranog i da bude 7 dana od kreiranja
            var userId = _userManager.GetUserId(User);
            obj.CompanyId = userId;
            obj.CreatedDate = DateTime.Now;
            var startDate = obj.StartDate.Date;
            var endDate = obj.EndDate.Date;
            obj.StartDate = startDate;
            obj.EndDate = endDate;
            _internshipRepo.Add(obj);
            _internshipRepo.Save();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Company")]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Internship? InternshipFromDb1 = _internshipRepo.Get(x => x.InternshipId == id); ;

            if (InternshipFromDb1 == null)
            {
                return NotFound();
            }
            return View(InternshipFromDb1);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Edit(Internship obj)
        {
            if (ModelState.IsValid)
            {
                var startDate = obj.StartDate.Date;
                var endDate = obj.EndDate.Date;
                obj.StartDate = startDate;
                obj.EndDate = endDate;
                _internshipRepo.Update(obj);
                _internshipRepo.Save();
                return RedirectToAction("Index");
            }
            return View();

        }
        [Authorize(Roles = "Admin,Company")]
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
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Internship? obj = await _internshipRepo.GetAsync(x => x.InternshipId == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                List<SavedInternship> savedInternships = (await _savedInternRepo.GetAllAsync(x => x.InternshipId == obj.InternshipId)).ToList();
                if (savedInternships.Any())
                {
                    await _savedInternRepo.RemoveRangeAsync(savedInternships);
                }

                List<InternshipApplication> internshipApplications = (await _internApplicationRepo.GetAllAsync(x => x.InternshipId == obj.InternshipId)).ToList();
                if (internshipApplications.Any())
                {
                    foreach (InternshipApplication application in internshipApplications)
                    {
                        List<Notification> notifications = (await _notificationRepository.GetAllAsync(x => x.InternshipApplicationId == application.InternshipApplicationId)).ToList();
                        if (notifications.Any())
                        {
                            await _notificationRepository.RemoveRangeAsync(notifications);
                        }
                    }

                    await _internApplicationRepo.RemoveRangeAsync(internshipApplications);
                }
            }

            await _internshipRepo.RemoveAsync(obj);
            await _internshipRepo.SaveAsync();

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

            if (!string.IsNullOrEmpty(location) && location != "-")
            {
               // internships = internships.Where(x => x.Location.Contains(location)).ToList();
                internships = internships.Where(x => x.Location==location).ToList();
            }

            if (!string.IsNullOrEmpty(position) && position != "-")
            {
                internships = internships.Where(x => x.Position == position).ToList();
            }

            if (!string.IsNullOrEmpty(technology) && technology != "-")
            {
                internships = internships.Where(x => x.Technology == technology).ToList();
            }

            if (!string.IsNullOrEmpty(language) && language != "-")
            {
                internships = internships.Where(x => x.ProgLanguage == language).ToList();
            }

            if (!string.IsNullOrEmpty(workPlace) && workPlace != "-")
            {
                internships = internships.Where(x => x.WorkPlace == workPlace).ToList();
            }

            return View("Index", internships);
        }

        public IActionResult Search(string searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                List<Internship> results = _internshipRepo.GetAll(x => x.Title.Contains(searchTerm) || x.CompanyName == searchTerm || x.Description.Contains(searchTerm)).ToList();
                if (results.Count > 0)
                {
                    return View("Index", results);
                }
            }

            return View("Index");
        }


    }
}
