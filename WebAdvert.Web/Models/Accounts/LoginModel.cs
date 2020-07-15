using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.Accounts
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Passord is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Rememeber Me")]
        public bool RememberMe { get; set; }
    }
}
