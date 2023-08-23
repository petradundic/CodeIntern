using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<InternshipApplication> GetAsync(Expression<Func<InternshipApplication, bool>> predicate)
        {
            return await _db.InternshipApplication.FirstOrDefaultAsync(predicate);

        }

        public async Task RemoveAsync(InternshipApplication entity)
        {
            _db.InternshipApplication.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<InternshipApplication> entities)
        {
            _db.InternshipApplication.RemoveRange(entities);
            await _db.SaveChangesAsync();
        }
    }
}
