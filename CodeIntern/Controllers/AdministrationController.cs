using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using CodeIntern.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeIntern.Controllers
{
    public class AdministrationController : Controller
    {

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private ICompanyRepository _companyRepository;
        private IInternApplicationRepository _internApplicationRepository;
        private IInternshipRepository _internshipRepository;
        private ISavedInternRepository _savedInternRepository;
        private INotificationRepository _notificationRepository;



        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IInternApplicationRepository internApplicationRepository, IInternshipRepository internshipRepository, ISavedInternRepository savedInternRepository,ICompanyRepository companyRepository, INotificationRepository notificationRepository,SignInManager<IdentityUser> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _internApplicationRepository = internApplicationRepository;
            _internshipRepository = internshipRepository;
            _savedInternRepository = savedInternRepository;
            _companyRepository = companyRepository;
            _notificationRepository = notificationRepository;   
            _signInManager = signInManager;
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            //vidi jel bolje obrisat iz AspUsers prije ili nakon cilog procesa

            var user=await _userManager.FindByIdAsync(id);

            
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id={id} not found";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if(result.Succeeded)
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


                    return RedirectToAction("Index", "Home");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Index");
            }
         }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetData)
        {
            var userId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                if (resetData.OldPassword != null) 
                {
                    if(userId!=null)
                    { 
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                         return NotFound();
                     }

                    else
                    {

                        bool isOldPassCorrect = await _userManager.CheckPasswordAsync(user, resetData.OldPassword);
                    if (isOldPassCorrect) {
                                    if (!string.IsNullOrEmpty(resetData.NewPassword))
                                    {
                                        var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, resetData.NewPassword);
                                        user.PasswordHash = newPasswordHash;
                                    }

                                    // Save changes to the user
                                    var result = await _userManager.UpdateAsync(user);

                                    if (result.Succeeded)
                                    {
                                        // Optionally, sign the user out and back in to refresh the authentication cookie
                                        await _signInManager.SignOutAsync();
                                        await _signInManager.SignInAsync(user, isPersistent: false);

                                        return RedirectToAction("Index", "Home"); // Redirect to a success page
                                    }

                                    // Handle errors here if the update fails
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



        //    public async Task<IActionResult> UpdateUser(string id)
        //{
        //    // Find the user by their ID
        //    var user = await _userManager.FindByIdAsync(id);

        //    if (user == null)
        //    {
        //        // User not found
        //        return NotFound();
        //    }

        //    // Optionally, you can load the user's associated claims and roles here
        //    // var userClaims = await _userManager.GetClaimsAsync(user);
        //    // var userRoles = await _userManager.GetRolesAsync(user);

        //    return View(user); // Pass the user to the view for editing
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateUser(string id, IdentityUser updatedUser, string password)
        //{
        //    if (id != updatedUser.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByIdAsync(id);

        //        if (user == null)
        //        {
        //            return NotFound();
        //        }

        //        // Update user properties
        //        user.Email = updatedUser.Email;
        //        user.UserName = updatedUser.Email; // Assuming username is the same as email
        //                                           // You can add more updates here as needed

        //        // Update the user's password if it's provided
        //        if (!string.IsNullOrEmpty(password))
        //        {
        //            var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
        //            user.PasswordHash = newPasswordHash;
        //        }

        //        // Save changes to the user
        //        var result = await _userManager.UpdateAsync(user);

        //        if (result.Succeeded)
        //        {
        //            // Optionally, sign the user out and back in to refresh the authentication cookie
        //            await _signInManager.SignOutAsync();
        //            await _signInManager.SignInAsync(user, isPersistent: false);

        //            return RedirectToAction("Index", "Home"); // Redirect to a success page
        //        }

        //        // Handle errors here if the update fails
        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error.Description);
        //        }
        //    }

        //    // If ModelState is not valid, return to the edit view with validation errors
        //    return View(updatedUser);
        //}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UsersList()
        {
            var users = _userManager.Users.ToList(); 
            return View(users);
        }

        
    }
}
