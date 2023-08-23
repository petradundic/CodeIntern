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
        private ICompanyRepository _companyRepository;
        private IInternApplicationRepository _internApplicationRepository;
        private IInternshipRepository _internshipRepository;
        private ISavedInternRepository _savedInternRepository;
        private INotificationRepository _notificationRepository;



        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IInternApplicationRepository internApplicationRepository, IInternshipRepository internshipRepository, ISavedInternRepository savedInternRepository,ICompanyRepository companyRepository, INotificationRepository notificationRepository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _internApplicationRepository = internApplicationRepository;
            _internshipRepository = internshipRepository;
            _savedInternRepository = savedInternRepository;
            _companyRepository = companyRepository;
            _notificationRepository = notificationRepository;   
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
