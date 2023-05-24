using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _companyRepo;
        public CompanyController(ICompanyRepository db) 
        {
            _companyRepo = db;
        }
        public IActionResult Index()
        {
            List<Company> companiesList = _companyRepo.GetAll().ToList();
            return View(companiesList);
        }
        public IActionResult Details(int id)
        {
            Company? CompanyFromDb = _companyRepo.Get(x=>x.CompanyId==id);
            return View(CompanyFromDb);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Company obj)
        {
            obj.RegistrationRequest = true;
            obj.RegistrationReqDate = DateTime.Now;
            _companyRepo.Add(obj);
            _companyRepo.Save();  
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Company? CompanyFromDb = _unitOfWork.Company.Get(u => u.Id == id);
            Company? CompanyFromDb1 = _companyRepo.Get(u=>u.CompanyId==id);
            //Company? CompanyFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

            if (CompanyFromDb1 == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb1);
        }
        [HttpPost]
        public IActionResult Edit(Company obj)
        {
            if (ModelState.IsValid)
            {
                _companyRepo.Update(obj);
                _companyRepo.Save();
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company? CompanyFromDb = _companyRepo.Get(x=>x.CompanyId==id);

            if (CompanyFromDb == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb);
        }
        [HttpPost, ActionName("Delete")]
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
    }
}
