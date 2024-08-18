using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using CodeIntern.Utility;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public InternshipApplicationController(IInternApplicationRepository db, IInternshipRepository internshipRepository, UserManager<ApplicationUser> userManager, INotificationRepository notificationRepository)
        {
            _internApplicationRepo = db;
            _internshipRepository = internshipRepository;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }
        [Authorize(Roles = "Admin,Company,Student")]
        public async Task<IActionResult> Index(int? internshipId, string? studentId)
        {
            List<InternshipApplication> applications = null;
            List<ApplicationIndexViewModel> vm = new List<ApplicationIndexViewModel>();

            if (internshipId != null)
            {
                applications = _internApplicationRepo.GetAll(x => x.InternshipId == internshipId).ToList();
                vm = await GetInternshipApplicationsViewModelListAsync(applications);
            }
            else if (!string.IsNullOrEmpty(studentId))
            {
                applications = _internApplicationRepo.GetAll(x => x.StudentId == studentId).ToList();
                vm = await GetInternshipApplicationsViewModelListAsync(applications);
            }
            else
            {
                applications = _internApplicationRepo.GetAll().ToList();
                vm = await GetInternshipApplicationsViewModelListAsync(applications);
            }

            return View(vm);
        }


        public async Task<List<ApplicationIndexViewModel>> GetInternshipApplicationsViewModelListAsync(List<InternshipApplication> applications)
        {
            List<ApplicationIndexViewModel> vmList = new List<ApplicationIndexViewModel>();

            foreach (var item in applications)
            {
                var user = await _userManager.FindByIdAsync(item.StudentId);
                vmList.Add(new ApplicationIndexViewModel(item, user));
            }

            return vmList;
        }

        public IActionResult MyInternApplications()
        {
            var userId = _userManager.GetUserId(User);
            return RedirectToAction("Index", new { studentId = userId });
        }

        [Authorize(Roles = "Admin, Company, Student")]
        public async Task<IActionResult> Details(int id, int notificationId)
        {
            InternshipApplication? internshipApplication = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);
            if (internshipApplication == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(internshipApplication.StudentId);
            if (user == null)
            {
                return NotFound();
            }
            ApplicationIndexViewModel vm = new ApplicationIndexViewModel(internshipApplication, user);

            Notification notification = _notificationRepository.Get(x => x.NotificationId == notificationId);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            _notificationRepository.Update(notification);
            _notificationRepository.Save();

            return View(vm);
        }


        [Authorize(Roles = "Admin, Student")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            ApplicationViewModel vm=new ApplicationViewModel();
            vm.FirstName = user.FirstName;
            vm.LastName=user.LastName;
            vm.Email = user.Email;
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Student")]
        public IActionResult Create(ApplicationViewModel temp, int InternshipId)
        {
            if (temp.CvFile != null)
            {
                temp.FileExtension = Path.GetExtension(temp.CvFile.FileName).ToLower();

                if (temp.FileExtension != ".pdf" && temp.FileExtension != ".zip")
                {
                    ModelState.AddModelError("CvFile", "Only PDF or ZIP files are allowed.");
                    return View(temp);  
                }

                using (var memoryStream = new MemoryStream())
                {
                    temp.CvFile.CopyTo(memoryStream);
                    byte[] cvBytes = memoryStream.ToArray();
                    var userId = _userManager.GetUserId(User);
                    var directoryPath = Path.Combine(SD.CvPath, InternshipId.ToString(), userId);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = $"{temp.FirstName}_{temp.LastName}{temp.FileExtension}";
                    var newFilePath = Path.Combine(directoryPath, fileName);

                    System.IO.File.WriteAllBytes(newFilePath, cvBytes);

                    InternshipApplication obj = new InternshipApplication();

                    Internship internship = _internshipRepository.Get(x => x.InternshipId == InternshipId);

                    obj.InternshipId = InternshipId;
                    obj.StudentId = userId;
                    obj.DateCreated = DateTime.Now;
                    obj.Status = "Applied";
                    obj.InternshipTitle = internship.Title;
                    obj.CVPath = newFilePath;
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
                return View(temp);  
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

            string filePath = obj.CVPath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string? fileExtension = Path.GetExtension(filePath)?.ToLower();
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
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            InternshipApplication internshipApplication = _internApplicationRepo.Get(u => u.InternshipApplicationId == id);

            if (internshipApplication == null)
            {
                return NotFound();
            }
            ApplicationEditViewModel vm = new ApplicationEditViewModel(internshipApplication, user);

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Company, Student")]
        public async Task<IActionResult> Edit(ApplicationEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                InternshipApplication internshipApplication = _internApplicationRepo.Get(u => u.InternshipApplicationId == model.InternshipApplicationId);
                if (internshipApplication == null)
                {
                    return NotFound();
                }
                if (model.CvFile != null)
                {
                    model.FileExtension = Path.GetExtension(model.CvFile.FileName).ToLower();

                    if (model.FileExtension != ".pdf" && model.FileExtension != ".zip")
                    {
                        ModelState.AddModelError("CvFile", "Only PDF or ZIP files are allowed.");
                        return View(model);  
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await model.CvFile.CopyToAsync(memoryStream);
                        byte[] cvBytes = memoryStream.ToArray();

                        var user = await _userManager.GetUserAsync(User);
                        if (user == null)
                        {
                            return Unauthorized();
                        }

                        var directoryPath = Path.Combine(SD.CvPath, internshipApplication.InternshipId.ToString(), user.Id);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        var fileName = $"{model.FirstName}_{model.LastName}{model.FileExtension}";
                        var newFilePath = Path.Combine(directoryPath, fileName);

                        if (!string.IsNullOrEmpty(internshipApplication.CVPath) && System.IO.File.Exists(internshipApplication.CVPath))
                        {
                            System.IO.File.Delete(internshipApplication.CVPath);
                        }

                        await System.IO.File.WriteAllBytesAsync(newFilePath, cvBytes);

                        internshipApplication.CVPath = newFilePath;
                        internshipApplication.FileExtension = model.FileExtension;
                    }
                }

                if (!string.IsNullOrEmpty(model.SelectedStatus) && model.SelectedStatus != internshipApplication.Status)
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
                else
                {
                    internshipApplication.Status = "Applied";
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
            // Retrieve the internship application by ID
            InternshipApplication? obj = await _internApplicationRepo.GetAsync(x => x.InternshipApplicationId == id);
            if (obj == null)
            {
                return NotFound();
            }

            // Retrieve the associated internship
            Internship internship = _internshipRepository.Get(x => x.InternshipId == obj.InternshipId);
            if (internship == null)
            {
                return NotFound();
            }

            // Retrieve all notifications related to the internship application
            List<Notification> notifications = _notificationRepository.GetAll(x => x.InternshipApplicationId == obj.InternshipApplicationId).ToList();
            if (notifications.Any())
            {
                await _notificationRepository.RemoveRangeAsync(notifications);
            }

            // Delete the CV file if it exists
            if (!string.IsNullOrEmpty(obj.CVPath) && System.IO.File.Exists(obj.CVPath))
            {
                System.IO.File.Delete(obj.CVPath);
            }

            // Remove the internship application
            _internApplicationRepo.Remove(obj);
            await _internApplicationRepo.SaveAsync();

            // Update the number of applications for the internship
            internship.NumOfApplications -= 1;
            _internshipRepository.Update(internship);
            await _internshipRepository.SaveAsync();

            return RedirectToAction("Index", "Internship");
        }

    }
}
