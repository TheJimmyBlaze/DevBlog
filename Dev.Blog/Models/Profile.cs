using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dev.Blog.Models
{
    public class Profile
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public string ImageName { get; set; }
        public string Blurb { get; set; }

        public string Email { get; set; }
        public string Github { get; set; }
        public string LinkedIn { get; set; }

        public IWebHostEnvironment Environment { get; set; }

        public string GetImagePath()
        {
            const string COLLECT_ERROR = "Failed to collect Profile Image!";
            const string IMAGE_PATH = @"images\profiles";

            if (string.IsNullOrEmpty(ImageName))
                throw new ArgumentException(string.Format("{0} Image path was empty for profile: {1}", COLLECT_ERROR, Name));

            string path = Path.Combine(IMAGE_PATH, ImageName);
            if (!File.Exists(Path.Combine(Environment.WebRootPath, path)))
                throw new FileNotFoundException(string.Format("{0} Image path: {1} could not be found for profile: {1}", COLLECT_ERROR, path, Name));

            return path;
        }

        public string GetMailToAddress()
        {
            const string MAIL_TO = "mailto";
            return string.Format("{0}:{1}", MAIL_TO, Email);
        }
    }
}
