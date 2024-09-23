using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_v2.Models
{
        public class Registration
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public string AccountType { get; set; }
            public string Code { get; set; }
            public string UserName { get; set; }

            public int? ProjectId { get; set; }
            public virtual Project Project { get; set; }

            public bool IsActive { get; set; }
            public bool IsUpdated { get; set; }

            public int? RegistrationRequestId { get; set; }

            public string ContactNumber { get; set; }
            public int? DeactivatedByRegistrationId { get; set; }
            public string DeactivatedRemarks { get; set; }
            public int? SessionPrivilegeId { get; set; }


        }

        public class RegistrationDetailsViewModel
        {
            public Registration Registration { get; set; }
            public RegistrationRequest RegistrationRequest { get; set; }
            public List<UserPrivilege> UserPrivileges { get; set; }
        }

        public class RegistrationCreateViewModel
        {
            public Registration Registration { get; set; }
            public RegistrationRequest RegistrationRequest { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public DateTime? ExpiryDate { get; set; }

            public bool IsRegistration { get; set; }
            public string AccountType { get; set; }
            public string Code { get; set; }
            public string UserName { get; set; }
        }

        public class RegistrationVerifyAccountModel
        {
            public int Id { get; set; }
            public virtual RegistrationRequest RegistrationRequest { get; set; }
        }

        public class RegistrationIndexViewModel
        {
            public List<Registration> Registrations { get; set; }
        }

        public class Deactivation
        {
            public List<Registration> Registrations { get; set; }
            public DateTime DateDeactivated { get; set; }
            public int? RegistrationId { get; set; }
            public Registration Registration { get; set; }
            public int DeactivatedByRegistrationId { get; set; }
            public Registration DeactivatedByRegistration { get; set; }
            public string Remark { get; set; }
        
        }
}