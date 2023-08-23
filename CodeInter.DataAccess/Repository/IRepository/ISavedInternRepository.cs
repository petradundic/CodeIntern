using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository.IRepository
{
    public interface ISavedInternRepository : IRepository<SavedInternship>
    {
        void Save();
        Task<SavedInternship?> GetAsync(Expression<Func<SavedInternship, bool>> predicate);
        Task RemoveAsync(SavedInternship entity);
        Task SaveAsync();
        Task RemoveRangeAsync(List<SavedInternship> entities);

    }
}
