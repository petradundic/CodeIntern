using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using CodeIntern.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using CodeIntern.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodeIntern.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RegisterModel _registerModel;
        public CompanyController(ICompanyRepository db, IWebHostEnvironment webHostEnvironment, RegisterModel registerModel)
        {
            _companyRepo = db;
            _webHostEnvironment = webHostEnvironment;
            _registerModel = registerModel; 
        }
        public IActionResult Index(List<Company>? obj)
        {
            IEnumerable<SelectListItem> locations = _companyRepo.GetAll().Select(x => x.City).Distinct().OrderBy(city => city).Select(city => new SelectListItem { Text = city, Value = city });
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
                string companyLogoPath=Path.Combine(wwwRootPath, @"images\company_logo");

                using(var fileStream = new FileStream(Path.Combine(companyLogoPath, fileName), FileMode.Create))
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

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Company? CompanyFromDb = _unitOfWork.Company.Get(u => u.Id == id);
            Company? CompanyFromDb1 = _companyRepo.Get(u => u.CompanyId == id);
            //Company? CompanyFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

            if (CompanyFromDb1 == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb1);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Edit(Company obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string companyLogoPath = Path.Combine(wwwRootPath, @"images\company_logo");

                    if(!string.IsNullOrEmpty(obj.ImageUrl)) 
                    {
                        var oldImageUrl = Path.Combine(wwwRootPath, obj.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImageUrl))
                        {
                            System.IO.File.Delete(oldImageUrl);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(companyLogoPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.ImageUrl = @"images\company_logo\" + fileName;
                }
                _companyRepo.Update(obj);
                _companyRepo.Save();
                return RedirectToAction("Index");
            }
            return View();

        }
        [Authorize(Roles = "Admin,Company")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company? CompanyFromDb = _companyRepo.Get(x => x.CompanyId == id);

            if (CompanyFromDb == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb);
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

        public IActionResult Filter(string? location)
        {
            List<Company> companies = _companyRepo.GetAll().ToList();


            if (!string.IsNullOrEmpty(location) && location != "-")
            {
                companies = companies.Where(x => x.City == location).ToList();
            }



            return View("Index", companies);
        }
    }
}
