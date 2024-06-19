using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIntern.Models
{
    public class EditCompanyViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
