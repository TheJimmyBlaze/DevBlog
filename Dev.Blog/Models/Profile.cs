using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev.Blog.Models
{
    public class Profile
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public string ImagePath { get; set; }
        public string Blurb { get; set; }

        public string Email { get; set; }
        public string Github { get; set; }
        public string LinkedIn { get; set; }
    }
}
