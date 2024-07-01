using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository.IRepository
{
    public interface IStudentProfileRepository : IRepository<StudentProfile>
    {
        void Update(StudentProfile obj);
        void Save();
        Task<StudentProfile?> GetAsync(Expression<Func<StudentProfile, bool>> predicate);
        Task RemoveAsync(StudentProfile entity);
        Task SaveAsync();

        Task RemoveRangeAsync(List<StudentProfile> entities);

    }
}
