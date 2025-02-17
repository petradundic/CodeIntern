﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeIntern.Models
{
    public class ApplicationEditViewModel
    {
        public int InternshipApplicationId { get; set; }
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Cv File (*.pdf/*.zip)")]
        public IFormFile? CvFile { get; set; }

        [Display(Name = "Application Status")]
        public string? SelectedStatus { get; set; }

        public string? FileExtension { get; set; }  // Add this property

        public ApplicationEditViewModel(InternshipApplication intApp, ApplicationUser student)
        {
            InternshipApplicationId = intApp.InternshipApplicationId;
            FirstName = student.FirstName;
            LastName = student.LastName;
            Email = student.Email;
        }
        public ApplicationEditViewModel()
        {
        }
    }
}
