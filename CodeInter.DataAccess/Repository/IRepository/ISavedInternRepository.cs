using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository.IRepository
{
    public interface ISavedInternRepository : IRepository<SavedInternship>
    {
        void Save();
    }
}
