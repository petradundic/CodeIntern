using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

    }
}
