using CodeIntern.DataAccess.Data;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.DataAccess.Repository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private ApplicationDbContext _db;
        public NotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task RemoveRangeAsync(List<Notification> entities)
        {
            _db.Notification.RemoveRange(entities);
            await _db.SaveChangesAsync();
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Notification obj) 
        {
            _db.Notification.Update(obj);
        }
    }
}
