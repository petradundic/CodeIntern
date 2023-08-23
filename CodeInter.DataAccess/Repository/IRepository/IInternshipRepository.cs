using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository.IRepository
{
    public interface IInternshipRepository : IRepository<Internship>
    {
        void Update(Internship obj);
        void Save();

    }
}
