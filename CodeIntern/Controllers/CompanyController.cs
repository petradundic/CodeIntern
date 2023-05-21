﻿using CodeIntern.DataAccess.Data;
using CodeIntern.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeIntern.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CompanyController(ApplicationDbContext db) 
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Company> companiesList = _db.Company.ToList();
            return View(companiesList);
        }
        public IActionResult Details(int id)
        {
            Company? CompanyFromDb = _db.Company.Find(id);
            return View(CompanyFromDb);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Company obj)
        {
            _db.Company.Add(obj);
            _db.SaveChanges();  
            return RedirectToAction("Index");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Company? CompanyFromDb = _unitOfWork.Company.Get(u => u.Id == id);
            Company? CompanyFromDb1 = _db.Company.FirstOrDefault(u=>u.CompanyId==id);
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
                _db.Company.Update(obj);
                _db.SaveChanges();
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
            Company? CompanyFromDb = _db.Company.Find(id);

            if (CompanyFromDb == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Company? obj = _db.Company.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Company.Remove(obj);
            _db.SaveChanges(true);
            TempData["success"] = "Company deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
