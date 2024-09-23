using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_v2.Models
{
    public class Home
    {
        public Registration Registration { get; set; }
        public DocumentCategory DocumentCategory { get; set; }
        public LinkedList<DocumentCategory> DocumentCategories { get; set; }
    }
}