using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_v2.Models
{
    public class UserPrivilege
    {
        public int Id { get; set; }
        public int? RegistrationId { get; set; }
        public virtual Registration Registration { get; set; }
        public int? PrivilegeId { get; set; }
        public virtual Privilege Privilege { get; set; }
    }
    public class UserPrivilegeDetailsViewModel
    {
        public int UserPrivilegeId { get; set; }
        public Registration Registration_Info { get; set; }
        public List<UserPrivilege> UserPrivilege_List { get; set; }

    }
}