using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class InternApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        public int InternshipId { get; set; }

        [Required]
        public string StudentId { get; set; }
        [Required]
        
        public string ResumeFile { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime AppliedDate { get; set; }
        [Required]
        public string StudentFirstName { get; set; }
        [Required]
        public string StudentLastName { get; set; }
        [Required]
        public string StudentEmail { get; set; }
        

    }
}
