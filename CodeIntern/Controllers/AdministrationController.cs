using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using CodeIntern.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeIntern.Controllers
{
    public class AdministrationController : Controller
    {

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        private ICompanyRepository _companyRepository;
        private IInternApplicationRepository _internApplicationRepository;
        private IInternshipRepository _internshipRepository;
        private ISavedInternRepository _savedInternRepository;
        private INotificationRepository _notificationRepository;
        private IStudentProfileRepository _studentProfileRepository;



        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IInternApplicationRepository internApplicationRepository, IInternshipRepository internshipRepository, ISavedInternRepository savedInternRepository, ICompanyRepository companyRepository, INotificationRepository notificationRepository, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, IStudentProfileRepository studentProfileRepository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _internApplicationRepository = internApplicationRepository;
            _internshipRepository = internshipRepository;
            _savedInternRepository = savedInternRepository;
            _companyRepository = companyRepository;
            _notificationRepository = notificationRepository;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _studentProfileRepository = studentProfileRepository;
        }

        [Authorize(Roles = "Admin,Student,Company")]
        public async Task<IActionResult> DeleteUser(string? id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(currentUserId);

            bool isAdmin = await _userManager.IsInRoleAsync(user, SD.Role_Admin);

            if (!String.IsNullOrEmpty(id))
                user = await _userManager.FindByIdAsync(id);
            else
                await _signInManager.SignOutAsync();

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id={id} not found";
                return View("NotFound");
            }
            else
            {
                bool isCompany = await _userManager.IsInRoleAsync(user, SD.Role_Company);
                bool isStudent = await _userManager.IsInRoleAsync(user, SD.Role_Student);

                if (isCompany)
                {
                    Company comp = _companyRepository.Get(x => x.Email == user.Email);
                    List<Internship> intern = _internshipRepository.GetAll(x => x.CompanyId == user.Id).ToList(); ;

                    if (intern.Any())
                    {
                        foreach (Internship internship in intern)
                        {
                            List<SavedInternship> savedInternships = _savedInternRepository.GetAll(x => x.InternshipId == internship.InternshipId).ToList();
                            if (savedInternships.Any())
                            {
                                await _savedInternRepository.RemoveRangeAsync(savedInternships);

                            }

                            List<InternshipApplication> internshipApplications = _internApplicationRepository.GetAll(x => x.InternshipId == internship.InternshipId).ToList();
                            if (internshipApplications.Any())
                            {
                                foreach (InternshipApplication application in internshipApplications)
                                {
                                    List<Notification> notifications = _notificationRepository.GetAll(x => x.InternshipApplicationId == application.InternshipApplicationId).ToList();
                                    if (notifications.Any())
                                    {
                                        await _notificationRepository.RemoveRangeAsync(notifications);
                                    }

                                }
                                await _internApplicationRepository.RemoveRangeAsync(internshipApplications);
                            }
                        }
                        _internshipRepository.RemoveRange(intern);
                        _internApplicationRepository.Save();

                    }

                    _companyRepository.Remove(comp);
                    _companyRepository.Save();
                }

                else if (isStudent)
                {
                    List<SavedInternship> savedInternships = _savedInternRepository.GetAll(x => x.StudentId == user.Id).ToList();
                    if (savedInternships.Any())
                    {
                        await _savedInternRepository.RemoveRangeAsync(savedInternships);

                    }

                    List<InternshipApplication> internshipApplications = _internApplicationRepository.GetAll(x => x.StudentId == user.Id).ToList();
                    if (internshipApplications.Any())
                    {
                        foreach (InternshipApplication application in internshipApplications)
                        {
                            List<Notification> notifications = _notificationRepository.GetAll(x => x.ToUser == user.Id).ToList();
                            if (notifications.Any())
                            {
                                await _notificationRepository.RemoveRangeAsync(notifications);

                            }
                        }

                        await _internApplicationRepository.RemoveRangeAsync(internshipApplications);

                    }
                }
                var result = await _userManager.DeleteAsync(user);

                if (isAdmin)
                    return RedirectToAction("UsersList");
                else
                    return RedirectToAction("Index", "Home");
            }
        }


        [Authorize(Roles = "Admin,Company,Student")]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Company,Student")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetData)
        {
            var userId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                if (resetData.OldPassword != null)
                {
                    if (userId != null)
                    {
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user == null)
                        {
                            return NotFound();
                        }

                        else
                        {

                            bool isOldPassCorrect = await _userManager.CheckPasswordAsync(user, resetData.OldPassword);
                            if (isOldPassCorrect)
                            {
                                if (!string.IsNullOrEmpty(resetData.NewPassword))
                                {
                                    var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, resetData.NewPassword);
                                    user.PasswordHash = newPasswordHash;
                                }

                                var result = await _userManager.UpdateAsync(user);

                                if (result.Succeeded)
                                {
                                    await _signInManager.SignOutAsync();
                                    await _signInManager.SignInAsync(user, isPersistent: false);

                                    return RedirectToAction("Index", "Home");
                                }

                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                            }
                        }
                    }
                }

            }


            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult ManageStudentProfile()
        {
            var userId = _userManager.GetUserId(User);
            StudentProfile student = _studentProfileRepository.Get(x => x.StudentId == userId);
            if (student == null)
                return RedirectToAction("CreateUserProfile");
            else
                return RedirectToAction("UpdateUserProfile");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Administration", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                    $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> UpdateUser(string id)
        {
            UpdateUserViewModel userVm = new UpdateUserViewModel();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            else
            {
                userVm.Id = user.Id;
                userVm.Email = user.Email;
            }
            return View(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {

            var user = await _userManager.FindByIdAsync(model.Id);
            bool isCompany = await _userManager.IsInRoleAsync(user, SD.Role_Company);


            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (isCompany)
                {
                    Company company = _companyRepository.Get(u => u.Email == user.Email);
                    company.Email = model.Email;
                    _companyRepository.Update(company);
                    _companyRepository.Save();
                }
                user.Email = model.Email;
                user.UserName = model.Email;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded && User.IsInRole(SD.Role_Admin))
                {
                    return View("UsersList");
                }

                if (result.Succeeded && !User.IsInRole(SD.Role_Admin))
                {
                    RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> UsersList(string? role)
        {
            var users = _userManager.Users.OrderBy(x => x.UserName).ToList();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            if (!string.IsNullOrEmpty(role))
            {
                if (role != "All")
                    users = users.Where(u => userRoles[u.Id].Contains(role)).OrderBy(x => x.UserName).ToList();
            }


            return View(users);
        }
        public IActionResult CreateUserProfile(int? internshipId)
        {
            var userId = _userManager.GetUserId(User);
            StudentProfile student = _studentProfileRepository.Get(x => x.StudentId == userId);
            if (student != null)
                return RedirectToAction("UpdateUserProfile", new {id= userId});
            else
            {
                ViewBag.IntershipId = internshipId;
                return View(); 
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Student")]
        public IActionResult CreateUserProfile(StudentProfileViewModel temp, int? internshipId)
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

                    StudentProfile obj = new StudentProfile();
                    var userId = _userManager.GetUserId(User);
                    obj.StudentId = userId;
                    obj.FirstName = temp.FirstName;
                    obj.LastName = temp.LastName;
                    obj.Email = temp.Email;
                    obj.FileExtension = temp.FileExtension;

                    // Save the file to the directory
                    var directoryPath = Path.Combine(SD.CvPath, userId);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = $"{temp.FirstName}_{temp.LastName}{temp.FileExtension}";
                    var filePath = Path.Combine(directoryPath, fileName);

                    System.IO.File.WriteAllBytes(filePath, cvBytes);

                    // Save the file path to the database
                    obj.CVPath = filePath;
                    _studentProfileRepository.Add(obj);
                    _studentProfileRepository.Save();

                    if (internshipId != null)
                        return RedirectToAction( "Details", "Internship", new { id = internshipId });
                    else
                        return RedirectToAction( "Index","Home");
                }
            }
            else
            {
                ModelState.AddModelError("CvFile", "File upload is required.");
                return View(temp);  // Return the view with the model error
            }
        }

        public IActionResult UpdateUserProfile(string id)
        {
            StudentProfile profile = _studentProfileRepository.Get(x => x.StudentId == id);
            if (profile == null)
            {
                return NotFound();
            }

            StudentProfileViewModel vm = new StudentProfileViewModel(profile);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Student")]
        public IActionResult UpdateUserProfile(StudentProfileViewModel temp)
        {
            // Get the existing student profile
            var userId = _userManager.GetUserId(User);
            StudentProfile existingProfile = _studentProfileRepository.Get(x => x.StudentId == userId);

            if (existingProfile == null)
            {
                return NotFound();
            }

            // Update the profile details
            existingProfile.FirstName = temp.FirstName;
            existingProfile.LastName = temp.LastName;
            existingProfile.Email = temp.Email;

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

                    // Save the file to the directory
                    var directoryPath = Path.Combine(SD.CvPath, userId);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = $"{temp.FirstName}_{temp.LastName}{temp.FileExtension}";
                    var newFilePath = Path.Combine(directoryPath, fileName);

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(existingProfile.CVPath) && System.IO.File.Exists(existingProfile.CVPath))
                    {
                        System.IO.File.Delete(existingProfile.CVPath);
                    }

                    // Write the new file
                    System.IO.File.WriteAllBytes(newFilePath, cvBytes);

                    // Update the file path and extension in the profile
                    existingProfile.CVPath = newFilePath;
                    existingProfile.FileExtension = temp.FileExtension;
                }
            }

            // Save the changes to the database
            _studentProfileRepository.Update(existingProfile);
            _studentProfileRepository.Save();

            return RedirectToAction("Index", "Home");
        }




    }
}
