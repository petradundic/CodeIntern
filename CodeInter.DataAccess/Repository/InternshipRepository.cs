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
    public class InternshipRepository : Repository<Internship>, IInternshipRepository
    {
        private ApplicationDbContext _db;
        public InternshipRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Internship?> GetAsync(Expression<Func<Internship, bool>> predicate)
        {
            return await _db.Internship.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<Internship>> GetAllAsync(Expression<Func<Internship, bool>> predicate)
        {
            return await _db.Internship.Where(predicate).ToListAsync();
        }

        public async Task RemoveAsync(Internship entity)
        {
            _db.Internship.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<Internship> entities)
        {
            _db.Internship.RemoveRange(entities);
            await _db.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Internship obj)
        {
            _db.Internship.Update(obj);
        }
    }
}
