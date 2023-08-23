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

        public async Task<SavedInternship> GetAsync(Expression<Func<SavedInternship, bool>> predicate)
        {
            return await _db.SavedInternship.FirstOrDefaultAsync(predicate);

        }

        public async Task RemoveAsync(SavedInternship entity)
        {
            _db.SavedInternship.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<SavedInternship> entities)
        {
            _db.SavedInternship.RemoveRange(entities);
            await _db.SaveChangesAsync();
        }
    }
}
