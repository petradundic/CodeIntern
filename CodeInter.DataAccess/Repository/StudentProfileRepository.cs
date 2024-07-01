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
    public class StudentProfileRepository : Repository<StudentProfile>, IStudentProfileRepository
    {
        private ApplicationDbContext _db;
        public StudentProfileRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(StudentProfile obj)
        {
            _db.StudentProfile.Update(obj);
        }

        public async Task<StudentProfile> GetAsync(Expression<Func<StudentProfile, bool>> predicate)
        {
            return await _db.StudentProfile.FirstOrDefaultAsync(predicate);

        }

        public async Task RemoveAsync(StudentProfile entity)
        {
            _db.StudentProfile.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<StudentProfile> entities)
        {
            _db.StudentProfile.RemoveRange(entities);
            await _db.SaveChangesAsync();
        }
    }
}