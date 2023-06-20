using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class SavedInternship
    {
        [Key]
        public int SavedInternshipId { get; set; }
        [Required]
        public int InternshipId { get; set; }
        [Required]

        public string StudentId { get; set; }
        [Required]
        public DateTime? DateSaved { get; set; }
    }
}
