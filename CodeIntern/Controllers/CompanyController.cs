using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using CodeIntern.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using CodeIntern.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace CodeIntern.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RegisterModel _registerModel;
        private readonly UserManager<IdentityUser> _userManager;
        public CompanyController(ICompanyRepository db, IWebHostEnvironment webHostEnvironment, RegisterModel registerModel, UserManager<IdentityUser> userManager)
        {
            _companyRepo = db;
            _webHostEnvironment = webHostEnvironment;
            _registerModel = registerModel;
            _userManager = userManager;
        }
        public IActionResult Index(List<Company>? obj)
        {
            IEnumerable<SelectListItem> locations = _companyRepo.GetAll(u=> u.RegistrationRequest==false).Select(x => x.City).Distinct().OrderBy(city => city).Select(city => new SelectListItem { Text = city, Value = city });
            ViewBag.Locations = locations;

            if (obj != null && obj.Count > 0)
            {
                return View(obj);
            }

            List<Company> companiesList = _companyRepo.GetAll(x => x.RegistrationRequest == false).ToList();

            if (User.IsInRole(SD.Role_Admin))
            {
                companiesList = _companyRepo.GetAll().ToList();
            }

            return View(companiesList);
        }

        public IActionResult Details(int id)
        {
            Company? CompanyFromDb = _companyRepo.Get(x => x.CompanyId == id);

            return View(CompanyFromDb);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Company obj, IFormFile? file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string companyLogoPath = Path.Combine(wwwRootPath, @"images\company_logo");

                using (var fileStream = new FileStream(Path.Combine(companyLogoPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.ImageUrl = @"images\company_logo\" + fileName;
            }
            obj.RegistrationRequest = true;
            obj.RegistrationReqDate = DateTime.Now;
            _companyRepo.Add(obj);
            _companyRepo.Save();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RegisterCompany(int companyId)
        {
            var result = await _registerModel.RegisterCompanyUser(companyId);
            return result;
        }

        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> Edit(int? id, string? companyUserId)
        {
            if (id == null && string.IsNullOrEmpty(companyUserId))
            {
                return NotFound();
            }

            Company company = null;

            if (!string.IsNullOrEmpty(companyUserId))
            {
                var user = await _userManager.FindByIdAsync(companyUserId);
                if (user == null)
                {
                    return NotFound();
                }
                company = _companyRepo.Get(u => u.Email == user.Email);
            }
            else if (id.HasValue)
            {
                company = _companyRepo.Get(u => u.CompanyId == id.Value);
            }

            if (company == null)
            {
                return NotFound();
            }

            // Map Company model to EditCompanyViewModel
            var editViewModel = new EditCompanyViewModel
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                Email = company.Email,
                Website = company.Website,
                Country = company.Country,
                City = company.City,
                Address = company.Address,
                Industry = company.Industry,
                Description = company.Description
            };

            return View(editViewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Edit(EditCompanyViewModel viewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Map EditCompanyViewModel to Company model
                var company = _companyRepo.Get(x=>x.CompanyId == viewModel.CompanyId);


                company.CompanyId = viewModel.CompanyId;
                company.CompanyName = viewModel.CompanyName;
                company.Email = viewModel.Email;
                company.Website = viewModel.Website;
                company.Country = viewModel.Country;
                company.City = viewModel.City;
                company.Address = viewModel.Address;
                company.Industry = viewModel.Industry;
                company.Description = viewModel.Description;

                if (file != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string companyLogoPath = Path.Combine(wwwRootPath, @"images\company_logo");

                    if (!string.IsNullOrEmpty(company.ImageUrl))
                    {
                        var oldImageUrl = Path.Combine(wwwRootPath, company.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImageUrl))
                        {
                            System.IO.File.Delete(oldImageUrl);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(companyLogoPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    company.ImageUrl = @"images\company_logo\" + fileName;
                }
                _companyRepo.Update(company);
                _companyRepo.Save();
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Company")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company? company = _companyRepo.Get(x => x.CompanyId == id);

            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Company,Student")]
        public IActionResult DeletePOST(int? id)
        {
            Company? obj = _companyRepo.Get(x => x.CompanyId == id);
            if (obj == null)
            {
                return NotFound();
            }
            _companyRepo.Remove(obj);
            _companyRepo.Save();
            TempData["success"] = "Company deleted successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Filter(string? location, string? registration)
        {
            List<Company> companies = _companyRepo.GetAll().ToList();

            if (!string.IsNullOrEmpty(location) && location != "-")
            {
                companies = companies.Where(x => x.City == location).ToList();
            }

            if (!string.IsNullOrEmpty(registration))
            {
                if (registration == "Registered")
                {
                    companies = companies.Where(x => x.RegistrationRequest == false).ToList();
                }
                else if (registration == "Waiting")
                {
                    companies = companies.Where(x => x.RegistrationRequest == true).ToList();
                }
            }

            return RedirectToAction("Index", companies);
        }


    }
}
