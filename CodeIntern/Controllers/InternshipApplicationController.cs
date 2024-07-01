using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static iTextSharp.text.pdf.AcroFields;
using static System.Net.Mime.MediaTypeNames;

namespace CodeIntern.Controllers
{
    public class InternshipApplicationController : Controller
    {
        private readonly IInternApplicationRepository _internApplicationRepo;
        private readonly IInternshipRepository _internshipRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public InternshipApplicationController(IInternApplicationRepository db, IInternshipRepository internshipRepository, UserManager<IdentityUser> userManager, INotificationRepository notificationRepository, IStudentProfileRepository studentProfileRepository)
        {
            _internApplicationRepo = db;
            _internshipRepository = internshipRepository;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
            _studentProfileRepository = studentProfileRepository;
        }
        [Authorize(Roles = "Admin,Company,Student")]
        public IActionResult Index(int? internshipId, string? studentId)
        {
            List<InternshipApplication> applications = null;
            List<ApplicationIndexViewModel> vm = new List<ApplicationIndexViewModel>();
            if (internshipId != null)
            {
                applications = _internApplicationRepo.GetAll(x => x.InternshipId == internshipId).ToList();
                vm= GetInternshipApplicationsViewModelList(applications);
            }
            else if (!String.IsNullOrEmpty(studentId))
            {
                applications = _internApplicationRepo.GetAll(x => x.StudentId == studentId).ToList();
                vm = GetInternshipApplicationsViewModelList(applications);
            }
            else
            {
                applications = _internApplicationRepo.GetAll().ToList();
                vm = GetInternshipApplicationsViewModelList(applications);
            }
            return View(vm);
        }

        public List<ApplicationIndexViewModel> GetInternshipApplicationsViewModelList(List<InternshipApplication> applications)
        {
            List<ApplicationIndexViewModel> vmList = new List<ApplicationIndexViewModel>();

            foreach (var item in applications)
            {
                StudentProfile profile = _studentProfileRepository.Get(x => x.StudentId == item.StudentId);
                vmList.Add(new ApplicationIndexViewModel(item,profile));
            }
            return vmList;
        }
        public IActionResult MyInternApplications()
        {
            var userId = _userManager.GetUserId(User);
            return RedirectToAction("Index", new { studentId = userId });
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult Details(int id, int notificationId)
        {
            InternshipApplication? internshipApplication = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);
            StudentProfile student=_studentProfileRepository.Get(x => x.StudentId == internshipApplication.StudentId);
            ApplicationIndexViewModel vm=new ApplicationIndexViewModel(internshipApplication, student);
            Notification notification = _notificationRepository.Get(x => x.NotificationId == notificationId);
            notification.IsRead = true;
            _notificationRepository.Update(notification);
            _notificationRepository.Save();

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Student")]
        public async Task<IActionResult> Create(int internshipId)
        {
            InternshipApplication obj = new InternshipApplication();
            Internship? internship = await _internshipRepository.GetAsync(x => x.InternshipId == internshipId);
            var userId = _userManager.GetUserId(User);
            StudentProfile? student = await _studentProfileRepository.GetAsync(x => x.StudentId == userId);

            if (student == null)
            {
                ViewBag.IntershipId = internshipId;
                return RedirectToAction("CreateUserProfile", "Administration");
            }

            obj.InternshipId = internshipId;
            obj.StudentId = userId;
            obj.DateCreated = DateTime.Now;
            obj.Status = "Applied";
            obj.InternshipTitle = internship.Title;

            _internApplicationRepo.Add(obj);
            await _internApplicationRepo.SaveAsync();
            internship.NumOfApplications += 1;
            _internshipRepository.Update(internship);
            await _internshipRepository.SaveAsync();
            return RedirectToAction("Details", "Internship", new { id = obj.InternshipId });
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult ViewPdf(int id)
        {
            InternshipApplication obj = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);

            if (obj == null)
            {
                return NotFound();
            }

            StudentProfile student = _studentProfileRepository.Get(x => x.StudentId == obj.StudentId);
            if (student == null || string.IsNullOrEmpty(student.CVPath))
            {
                return NotFound();
            }

            string filePath = student.CVPath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileExtension = Path.GetExtension(filePath)?.ToLower();
            string mimeType;

            switch (fileExtension)
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    return File(fileBytes, mimeType);
                case ".zip":
                    mimeType = "application/zip";
                    return File(fileBytes, mimeType, "download.zip");
                default:
                    return BadRequest("Unsupported file format");
            }
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult Edit(int id)
        {
            InternshipApplication internshipApplication = _internApplicationRepo.Get(u => u.InternshipApplicationId == id);
            if (internshipApplication == null)
            {
                return NotFound();
            }
            StudentProfile profile = _studentProfileRepository.Get(x => x.StudentId == internshipApplication.StudentId);
            ApplicationEditViewModel vm = new ApplicationEditViewModel(internshipApplication,profile);

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult Edit(ApplicationEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                InternshipApplication internshipApplication = _internApplicationRepo.Get(u => u.InternshipApplicationId == model.InternshipApplicationId);
                if (internshipApplication == null)
                {
                    return NotFound();
                }
                if (!String.IsNullOrEmpty(model.SelectedStatus))
                {
                    if (model.SelectedStatus != internshipApplication.Status)
                    {
                        internshipApplication.Status = model.SelectedStatus;
                        Internship internship = _internshipRepository.Get(x => x.InternshipId == internshipApplication.InternshipId);
                        string studentId = internshipApplication.StudentId;

                        Notification notification = new Notification
                        {
                            InternshipApplicationId = internshipApplication.InternshipApplicationId,
                            FromUser = _userManager.GetUserId(User),
                            ToUser = studentId,
                            Text = $"Your application status for internship {internship.Title} has been changed to {model.SelectedStatus}.",
                            DateCreated = DateTime.Now,
                            IsRead = false
                        };

                        _notificationRepository.Add(notification);
                        _notificationRepository.Save();
                    }
                }
                else
                    internshipApplication.Status = "Applied";

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
            List<Notification> notifications = _notificationRepository.GetAll(x => x.InternshipApplicationId == obj.InternshipApplicationId).ToList();
            if (notifications.Any())
            {
                await _notificationRepository.RemoveRangeAsync(notifications);
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
