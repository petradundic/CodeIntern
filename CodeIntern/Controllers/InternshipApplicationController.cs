using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Index(int? internshipId, string? studentId)
        {
            List<InternshipApplication> applications = null;
            if(internshipId != null)
            {
                applications = _internApplicationRepo.GetAll(x=>x.InternshipId==internshipId).ToList();
            }
            else if(!String.IsNullOrEmpty(studentId))
            {
                applications = _internApplicationRepo.GetAll(x => x.StudentId == studentId).ToList();
            }
            else
            {
                applications = _internApplicationRepo.GetAll().ToList();
            }
            return View(applications);
        }
        [Authorize(Roles = "Admin,Student,Company")]
        public IActionResult Details(int id, int notificationId)
        {
            InternshipApplication? internshipApplicationFromDb = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);
            Notification notification = _notificationRepository.Get(x => x.NotificationId == notificationId);
            notification.IsRead = true;
            _notificationRepository.Update(notification);
            _notificationRepository.Save();
              
            return View(internshipApplicationFromDb);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
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
            obj.InternshipTitle=internship.Title;

            _internApplicationRepo.Add(obj);
            _internApplicationRepo.Save();
            internship.NumOfApplications += 1;
            _internshipRepository.Update(internship);
            _internshipRepository.Save();

            return RedirectToAction("Details", "Inernship", new { id = obj.InternshipId});
        }

        public IActionResult ViewPdf(int id)
        { 
            InternshipApplication obj= _internApplicationRepo.Get(X=> X.InternshipApplicationId==id);
            return File(obj.CV, "application/pdf");


        }



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

                    internshipApplication.CV=cvBytes;
                }

                if (model.SelectedStatus != internshipApplication.Status)
                {
                    //ubaci logiku za notifikacije
                    internshipApplication.Status = model.SelectedStatus;
                    Internship internship = _internshipRepository.Get(x => x.InternshipId == internshipApplication.InternshipId);
                    string companyId = internship.CompanyId;
                    string studentId = internshipApplication.StudentId;


                    Notification notification=new Notification();
                    notification.InternshipApplicationId=internshipApplication.InternshipApplicationId;
                    notification.FromUser = _userManager.GetUserId(User);
                    notification.ToUser=studentId;
                    notification.Text = $"Your application status for internsip {internship.Title} has been changed to {model.SelectedStatus}.";
                    notification.DateCreated=DateTime.Now;
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

            //public IActionResult Delete(int? id)
            //{
            //    if (id == null || id == 0)
            //    {
            //        return NotFound();
            //    }
            //    InternshipApplication? InternshipApplicationFromDb = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);

            //    if (InternshipApplicationFromDb == null)
            //    {
            //        return NotFound();
            //    }
            //    return View(InternshipApplicationFromDb);
            //}
            //[HttpPost, ActionName("Delete")]
            //public IActionResult DeletePOST(int? id)
            //{
            //    InternshipApplication? obj = _internApplicationRepo.Get(x => x.InternshipApplicationId == id);
            //    if (obj == null)
            //    {
            //        return NotFound();
            //    }
            //    _internApplicationRepo.Remove(obj);
            //    _internApplicationRepo.Save();
            //    TempData["success"] = " InternshipApplication deleted successfully";
            //    return RedirectToAction("Index");
            //}

            public async Task<IActionResult> Delete(int? id)
        {
            InternshipApplication? obj = await _internApplicationRepo.GetAsync(x => x.InternshipApplicationId == id);

            if (obj == null)
            {
                return NotFound();
            }

            _internApplicationRepo.Remove(obj);
            await _internApplicationRepo.SaveAsync();

            return RedirectToAction("Index");
        }
    }
}
