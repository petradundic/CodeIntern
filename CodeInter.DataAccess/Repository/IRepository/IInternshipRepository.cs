using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository.IRepository
{
    public interface IInternshipRepository : IRepository<Internship>
    {
        void Update(Internship obj);
        void Save();
        Task<Internship?> GetAsync(Expression<Func<Internship, bool>> predicate);
        Task<IEnumerable<Internship>> GetAllAsync(Expression<Func<Internship, bool>> predicate);
        Task RemoveAsync(Internship entity);
        Task RemoveRangeAsync(IEnumerable<Internship> entities);
        Task SaveAsync();
    }
}
