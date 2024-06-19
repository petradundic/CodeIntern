using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

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
        [HttpPost]
        [Authorize(Roles = "Admin, Student")]
        public IActionResult Create(ApplicationViewModel temp, int InternshipId)
        {
            // Check if a file is uploaded
            if (temp.CvFile != null)
            {
                // Extract the file extension
                temp.FileExtension = Path.GetExtension(temp.CvFile.FileName).ToLower();

                // Validate the file extension
                if (temp.FileExtension != ".pdf" && temp.FileExtension != ".zip")
                {
                    ModelState.AddModelError("CvFile", "Only PDF or ZIP files are allowed.");
                    return View(temp);  // Return the view with the model error
                }

                // Convert IFormFile to byte array
                using (var memoryStream = new MemoryStream())
                {
                    temp.CvFile.CopyTo(memoryStream);
                    byte[] cvBytes = memoryStream.ToArray();

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
                    obj.FileExtension = temp.FileExtension;

                    _internApplicationRepo.Add(obj);
                    _internApplicationRepo.Save();
                    internship.NumOfApplications += 1;
                    _internshipRepository.Update(internship);
                    _internshipRepository.Save();

                    return RedirectToAction("Details", "Internship", new { id = obj.InternshipId });
                }
            }
            else
            {
                ModelState.AddModelError("CvFile", "File upload is required.");
                return View(temp);  // Return the view with the model error
            }
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public IActionResult ViewPdf(int id)
        {
            InternshipApplication obj = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);

            if (obj == null)
            {
                return NotFound();
            }

            byte[] fileBytes = obj.CV;
            string fileExtension = obj.FileExtension?.ToLower();

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

        //public IActionResult ViewPdf(int id)
        //{
        //    InternshipApplication obj = _internApplicationRepo.Get(X => X.InternshipApplicationId == id);
        //    return File(obj.CV, "application/pdf");
        //}
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
                if (model.CvFile != null)
                {
                    // Extract the file extension
                    model.FileExtension = Path.GetExtension(model.CvFile.FileName).ToLower();

                    // Validate the file extension
                    if (model.FileExtension != ".pdf" && model.FileExtension != ".zip")
                    {
                        ModelState.AddModelError("CvFile", "Only PDF or ZIP files are allowed.");
                        return View(model);  // Return the view with the model error
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        model.CvFile.CopyTo(memoryStream);
                        byte[] cvBytes = memoryStream.ToArray();

                        internshipApplication.CV = cvBytes;
                        internshipApplication.FileExtension = model.FileExtension;
                    }
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
