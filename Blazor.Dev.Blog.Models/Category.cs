using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor.Dev.Blog.Models
{
    public class Category
    {
        public string CategoryNaturalID { get; set; }

        public string Github { get; set; }

        public string Name { get; set; }
        public DateTime CreationDateUTC { get; set; }
        public string Blurb { get; set; }

        public string ImageName { get; set; }
    }
}
