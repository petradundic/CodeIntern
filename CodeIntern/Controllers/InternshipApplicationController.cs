using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CodeIntern.Controllers
{
    public class InternshipApplicationController : Controller
    {
        private readonly IInternApplicationRepository _internApplicationRepo;
        private readonly IInternshipRepository _internshipRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public InternshipApplicationController(IInternApplicationRepository db, IInternshipRepository internshipRepository, UserManager<IdentityUser> userManager, INotificationRepository notificationRepository)
        {
            _internApplicationRepo = db;
            _internshipRepository = internshipRepository;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }
        [Authorize(Roles = "Admin,Company,Student")]
        public IActionResult Index(int? internshipId, string? studentId)
        {
            List<InternshipApplication> applications = null;
            if (internshipId != null)
            {
                applications = _internApplicationRepo.GetAll(x => x.InternshipId == internshipId).ToList();
            }
            else if (!String.IsNullOrEmpty(studentId))
            {
                applications = _internApplicationRepo.GetAll(x => x.StudentId == studentId).ToList();
            }
            else
            {
                applications = _internApplicationRepo.GetAll().ToList();
            }
            return View(applications);
        }

        public IActionResult MyInternApplications()
        {
            var userId = _userManager.GetUserId(User);
            return RedirectToAction("Index", new { studentId = userId });
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult Details(int id, int notificationId)
        {
            InternshipApplication? internshipApplicationFromDb = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);
            Notification notification = _notificationRepository.Get(x => x.NotificationId == notificationId);
            notification.IsRead = true;
            _notificationRepository.Update(notification);
            _notificationRepository.Save();

            return View(internshipApplicationFromDb);
        }
        [Authorize(Roles = "Admin, Student")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Student")]
        public IActionResult Create(ApplicationViewModel temp, int InternshipId)
        {

            // Convert IFormFile to byte array
            byte[] cvBytes = null;
            if (temp.CvFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    temp.CvFile.CopyTo(memoryStream);
                    cvBytes = memoryStream.ToArray();
                }
            }

            InternshipApplication obj = new InternshipApplication();

            Internship internship = _internshipRepository.Get(x => x.InternshipId == InternshipId);
            var userId = _userManager.GetUserId(User);

            obj.InternshipId = InternshipId;
            obj.StudentId = userId;
            obj.DateCreated = DateTime.Now;
            obj.FirstName = temp.FirstName;
            obj.LastName = temp.LastName;
            obj.Email = temp.Email;
            obj.CV = cvBytes;
            obj.Status = "Applied";
            obj.InternshipTitle = internship.Title;

            _internApplicationRepo.Add(obj);
            _internApplicationRepo.Save();
            internship.NumOfApplications += 1;
            _internshipRepository.Update(internship);
            _internshipRepository.Save();

            return RedirectToAction("Details", "Internship", new { id = obj.InternshipId });
        }
        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult ViewPdf(int id)
        {
            InternshipApplication obj = _internApplicationRepo.Get(X => X.InternshipApplicationId == id);
            return File(obj.CV, "application/pdf");
        }
        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult Edit(int id)
        {
            InternshipApplication internshipApplication = _internApplicationRepo.Get(u => u.InternshipApplicationId == id);
            if (internshipApplication == null)
            {
                return NotFound();
            }

            var editViewModel = new EditViewModel
            {
                InternshipApplicationId = internshipApplication.InternshipApplicationId,
                FirstName = internshipApplication.FirstName,
                LastName = internshipApplication.LastName,
                Email = internshipApplication.Email,
                CvFile = null,
                SelectedStatus = internshipApplication.Status
            };

            return View(editViewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                InternshipApplication internshipApplication = _internApplicationRepo.Get(u => u.InternshipApplicationId == model.InternshipApplicationId);
                if (internshipApplication == null)
                {
                    return NotFound();
                }
                internshipApplication.FirstName = model.FirstName;
                internshipApplication.LastName = model.LastName;
                internshipApplication.Email = model.Email;

                byte[] cvBytes = null;
                if (model.CvFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        model.CvFile.CopyTo(memoryStream);
                        cvBytes = memoryStream.ToArray();
                    }

                    internshipApplication.CV = cvBytes;
                }

                if (model.SelectedStatus != internshipApplication.Status)
                {
                    //ubaci logiku za notifikacije
                    internshipApplication.Status = model.SelectedStatus;
                    Internship internship = _internshipRepository.Get(x => x.InternshipId == internshipApplication.InternshipId);
                    string companyId = internship.CompanyId;
                    string studentId = internshipApplication.StudentId;


                    Notification notification = new Notification();
                    notification.InternshipApplicationId = internshipApplication.InternshipApplicationId;
                    notification.FromUser = _userManager.GetUserId(User);
                    notification.ToUser = studentId;
                    notification.Text = $"Your application status for internsip {internship.Title} has been changed to {model.SelectedStatus}.";
                    notification.DateCreated = DateTime.Now;
                    notification.IsRead = false;

                    _notificationRepository.Add(notification);
                    _notificationRepository.Save();

                }

                _internApplicationRepo.Update(internshipApplication);
                _internApplicationRepo.Save();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public async Task<IActionResult> Delete(int? id)
        {
            InternshipApplication? obj = await _internApplicationRepo.GetAsync(x => x.InternshipApplicationId == id);
            Internship internship = _internshipRepository.Get(x => x.InternshipId == obj.InternshipId);
            if (obj == null)
            {
                return NotFound();
            }

            _internApplicationRepo.Remove(obj);
            await _internApplicationRepo.SaveAsync();
            internship.NumOfApplications -= 1;
            _internshipRepository.Update(internship);
            _internshipRepository.Save();

            return RedirectToAction("Index", "Internship");
        }
    }
}
