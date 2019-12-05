using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor.Dev.Blog.Models
{
    public class Post
    {
        public string PostNaturalID { get; set; }

        public string Title { get; set; }
        public DateTime CreationDateUTC { get; set; }

        public string ImageName { get; set; }
        public string Blurb { get; set; }
        public string Body { get; set; }
        public List<string> Tags { get; set; }
    }
}
