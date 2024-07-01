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
    public class StudentProfileViewModel
    {
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Cv File (*.pdf/*.zip)")]
        public IFormFile? CvFile { get; set; }
        public string? FileExtension { get; set; }

        public StudentProfileViewModel()
        {

        }
        public StudentProfileViewModel(StudentProfile profile)
        {
            FirstName=profile.FirstName;
            LastName=profile.LastName;
            Email=profile.Email;
        }

    }
}
