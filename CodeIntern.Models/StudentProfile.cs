using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeIntern.Models
{
    public class StudentProfile
    {
        [Key]
        public int ProfileId { get; set; }
        [Required]
        public string StudentId { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string CVPath { get; set; }
        public string? FileExtension { get; set; }
    }
}
