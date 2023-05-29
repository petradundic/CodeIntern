using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository
{
    public class InternApplicationRepository :Repository<InternshipApplication>, IInternApplicationRepository
    {
        private ApplicationDbContext _db;
        public InternApplicationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(InternshipApplication obj)
        {
            _db.InternshipApplication.Update(obj);
        }
    }
}
