using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_v2.Models
{
    public class TemplateViewModel
    {
        public Registration Registration { get; set; }
        public UserPrivilege UserPrivilege { get; set; }
        public List<UserPrivilege> UserPrivileges { get; set; }

        public string ControllerName { get; set; }
        public string ActionName { get; set; }

    }
}