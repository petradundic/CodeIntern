using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        [Required]
        public int InternshipApplicationId { get; set; }
        [Required]

        public string FromUser{ get; set; }
        [Required]
        public string ToUser { get; set; }
        
        public string? Text { get; set; }
        
        public DateTime? DateCreated { get; set; }
        public bool? IsRead { get; set; }
    }
}
