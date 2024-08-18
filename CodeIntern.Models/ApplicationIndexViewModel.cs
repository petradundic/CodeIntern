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
    public class ApplicationIndexViewModel
    {
        public int InternshipApplicationId { get; set; }
        public int InternshipId { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Internship Title")]
        public string? InternshipTitle { get; set; }

        [Display(Name = "Email")]

        public string? Email { get; set; }
        public string? Status { get; set; }

        public ApplicationIndexViewModel(InternshipApplication intApp, ApplicationUser student)
        {
            InternshipApplicationId = intApp.InternshipApplicationId;
            InternshipId=intApp.InternshipId;
            InternshipTitle = intApp.InternshipTitle;
            FirstName = student.FirstName;
            LastName = student.LastName;
            Email = student.Email;
            Status = intApp.Status;
        }
        public ApplicationIndexViewModel()
        {
        }
    }
}
