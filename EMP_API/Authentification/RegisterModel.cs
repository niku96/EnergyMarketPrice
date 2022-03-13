using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EMP_API.Authentification
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(60)]
        [MinLength(5)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(60)]
        [MinLength(5)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Birth date is required field")]
        [Range(typeof(DateTime), "1/1/1900", "1/1/2020", ErrorMessage = "Date is out of Range")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
