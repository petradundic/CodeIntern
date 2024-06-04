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



        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IInternApplicationRepository internApplicationRepository, IInternshipRepository internshipRepository, ISavedInternRepository savedInternRepository, ICompanyRepository companyRepository, INotificationRepository notificationRepository, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
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

    }
}
