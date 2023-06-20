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
    public class SavedInternRepository : Repository<SavedInternship>, ISavedInternRepository
    {
        private ApplicationDbContext _db;
        public SavedInternRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;   
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
