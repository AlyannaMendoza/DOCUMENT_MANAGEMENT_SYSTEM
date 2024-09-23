using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DMS_v2.Models
{
    public class RegistrationRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Code { get; set; }

        public bool AccountInformation { get; set; }

    }

    public class RegistrationRequestIndexViewModel
    {
        public List<RegistrationRequest> RegistrationRequests { get; set; }

    }

    public class RegistrationRequestCreateViewModel
    {

        public int Id { get; set; }
        public string MiddleName { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string sha { get; set; }

    }
}