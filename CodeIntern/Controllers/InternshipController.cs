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
using System.Collections.Generic;
using System.Data;

namespace CodeIntern.Controllers
{
    public class InternshipController : Controller
    {
        private readonly IInternshipRepository _internshipRepo;
        private readonly ISavedInternRepository _savedInternRepo;
        private readonly IInternApplicationRepository _internApplicationRepo;
        private INotificationRepository _notificationRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public InternshipController(UserManager<ApplicationUser> userManager, ISavedInternRepository savedInternRepo, IInternshipRepository internshipRepository, IInternApplicationRepository internApplicationRepo, INotificationRepository notificationRepository)
        {
            _userManager = userManager;
            _savedInternRepo = savedInternRepo;
            _internshipRepo = internshipRepository;
            _internApplicationRepo = internApplicationRepo;
            _notificationRepository = notificationRepository;
        }
        public IActionResult Index(string? companyId, string? companyName, List<Internship>? obj, bool? isSearch)
        {

            if (obj.Count == 0 && isSearch == true)
            {
                return View(obj);
            }


            List<Internship> internshipsList = _internshipRepo.GetAll().ToList();


            string tempLoc = String.Empty;
            string tempProgLang = String.Empty;
            string tempTech = String.Empty;

            foreach (var item in internshipsList)
            {
                tempLoc += item.Location;
                tempProgLang += item.ProgLanguage;
                tempTech += item.Technology;
            }

            IEnumerable<SelectListItem> technologies = tempTech.Split(", ").ToList()
            .Select(x => new SelectListItem { Text = x, Value = x })
            .DistinctBy(x => x.Text)
             .OrderBy(tech => tech.Text);

            IEnumerable<SelectListItem> positions = _internshipRepo.GetAll()
                .Where(x => x.Position != null)
                .Select(x => new SelectListItem { Text = x.Position, Value = x.Position })
                .DistinctBy(x => x.Text)
                .OrderBy(pos => pos.Text);

            IEnumerable<SelectListItem> progLanguages = tempProgLang.Split(", ").ToList()
                .Select(x => new SelectListItem { Text = x, Value = x })
                .DistinctBy(x => x.Text)
                .OrderBy(lang => lang.Text);

            IEnumerable<SelectListItem> locations = tempLoc.Split(", ").ToList()
                .Select(x => new SelectListItem { Text = x, Value = x })
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
            if (!string.IsNullOrEmpty(companyName))
            {
                internshipsList = _internshipRepo.GetAll(x => x.CompanyName == companyName).ToList();
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
            InternshipViewModel vm = new InternshipViewModel();
            var userId = _userManager.GetUserId(User);
            vm.CompanyId = userId;
            vm.StartDate = DateTime.Now.Date;
            vm.EndDate = DateTime.Now.Date;
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Create(InternshipViewModel obj)
        {
            var userId = _userManager.GetUserId(User);
            var currentDate = DateTime.Now;
            if (obj.StartDate < currentDate || obj.StartDate < currentDate.AddDays(7))
            {
                ModelState.AddModelError("StartDate", "Start date must be at least 7 days from today and not earlier than today.");
            }

            if (obj.EndDate < obj.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            Internship internship = new Internship
            {
                CompanyId = userId,
                CreatedDate = currentDate,
                StartDate = obj.StartDate.Date,
                EndDate = obj.EndDate.Date,
                IsPaid = obj.IsPaid,
                PayPerHour = obj.PayPerHour,
                Position = obj.Position,
                CompanyName = obj.CompanyName,
                Title = obj.Title,
                Description = obj.Description,
                NumberOfOpenings = obj.NumberOfOpenings,
                WorkPlace = obj.WorkPlace,
                ProgLanguage = obj.ProgLanguage != null ? string.Join(", ", obj.ProgLanguage) : string.Empty,
                Location = obj.Location != null ? string.Join(", ", obj.Location) : string.Empty,
                Technology = obj.Technology != null ? string.Join(", ", obj.Technology) : string.Empty
            };

            _internshipRepo.Add(internship);
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

            Internship internshipFromDb = _internshipRepo.Get(x => x.InternshipId == id);

            if (internshipFromDb == null)
            {
                return NotFound();
            }

            InternshipViewModel internshipViewModel = new InternshipViewModel
            {
                InternshipId = internshipFromDb.InternshipId,
                CompanyId = internshipFromDb.CompanyId,
                CreatedDate = internshipFromDb.CreatedDate,
                StartDate = internshipFromDb.StartDate,
                EndDate = internshipFromDb.EndDate,
                IsPaid = internshipFromDb.IsPaid,
                PayPerHour = internshipFromDb.PayPerHour,
                Position = internshipFromDb.Position,
                CompanyName = internshipFromDb.CompanyName,
                Title = internshipFromDb.Title,
                Description = internshipFromDb.Description,
                NumberOfOpenings = internshipFromDb.NumberOfOpenings,
                WorkPlace = internshipFromDb.WorkPlace,
                ProgLanguage = internshipFromDb.ProgLanguage?.Split(", ").ToList(),
                Location = internshipFromDb.Location?.Split(", ").ToList(),
                Technology = internshipFromDb.Technology?.Split(", ").ToList()
            };

            return View(internshipViewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Edit(InternshipViewModel obj)
        {
            var currentDate = DateTime.Now;
            if (obj.StartDate < currentDate || obj.StartDate < currentDate.AddDays(7))
            {
                ModelState.AddModelError("StartDate", "Start date must be at least 7 days from today and not earlier than today.");
            }

            if (obj.EndDate < obj.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                Internship internship = new Internship
                {
                    InternshipId = obj.InternshipId,
                    CompanyId = userId,
                    CreatedDate = currentDate,
                    StartDate = obj.StartDate.Date,
                    EndDate = obj.EndDate.Date,
                    IsPaid = obj.IsPaid,
                    PayPerHour = obj.PayPerHour,
                    Position = obj.Position,
                    CompanyName = obj.CompanyName,
                    Title = obj.Title,
                    Description = obj.Description,
                    NumberOfOpenings = obj.NumberOfOpenings,
                    WorkPlace = obj.WorkPlace,
                    ProgLanguage = obj.ProgLanguage != null ? string.Join(", ", obj.ProgLanguage) : string.Empty,
                    Location = obj.Location != null ? string.Join(", ", obj.Location) : string.Empty,
                    Technology = obj.Technology != null ? string.Join(", ", obj.Technology) : string.Empty
                };
                _internshipRepo.Update(internship);
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
                internships = internships.Where(x => x.Location.Contains(location)).ToList();
            }

            if (!string.IsNullOrEmpty(position) && position != "-")
            {
                internships = internships.Where(x => x.Position == position).ToList();
            }

            if (!string.IsNullOrEmpty(technology) && technology != "-")
            {
                internships = internships.Where(x => x.Technology.Contains(technology)).ToList();
            }

            if (!string.IsNullOrEmpty(language) && language != "-")
            {
                internships = internships.Where(x => x.ProgLanguage.Contains(language)).ToList();
            }

            if (!string.IsNullOrEmpty(workPlace) && workPlace != "-")
            {
                internships = internships.Where(x => x.WorkPlace == workPlace).ToList();
            }

            return RedirectToAction("Index", internships);
        }

        public IActionResult Search(string searchTerm)
        {

            if (!string.IsNullOrEmpty(searchTerm))
            {
                List<Internship> results = _internshipRepo.GetAll(x => x.Title.Contains(searchTerm) || x.CompanyName == searchTerm || x.Description.Contains(searchTerm)).ToList();

                if (results == null || results.Count == 0)
                {
                   return RedirectToAction("Index", new { obj = new List<Internship>(), isSearch = true });
                }
                else 
                    return RedirectToAction("Index", results);
            }


            return RedirectToAction("Index");
        }


    }
}
