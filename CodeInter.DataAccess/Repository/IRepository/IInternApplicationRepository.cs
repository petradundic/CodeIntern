using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository.IRepository
{
    public interface IInternApplicationRepository : IRepository<InternshipApplication>
    {
        void Update(InternshipApplication obj);
        void Save();
        Task<InternshipApplication?> GetAsync(Expression<Func<InternshipApplication, bool>> predicate);
        Task RemoveAsync(InternshipApplication entity);
        Task SaveAsync();


    }
}
