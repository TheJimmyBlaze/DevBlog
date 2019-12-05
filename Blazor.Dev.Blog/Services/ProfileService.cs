using Blazor.Dev.Blog.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blazor.Dev.Blog.Services
{
    public class ProfileService
    {
        private readonly IWebHostEnvironment environment;
        public ProfileService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<Profile> GetProfileAsync(string profileJsonPath)
        {
            const string COLLECT_ERROR = "Failed to collect profile!";
            const string PROFILE_PATH = @"data\profiles";

            string path = Path.Combine(environment.WebRootPath, PROFILE_PATH, profileJsonPath);
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("{0} Json path: {1} could not be found.", COLLECT_ERROR, path));

            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(stream);
            string jsonText = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(jsonText))
                throw new FileLoadException(string.Format("{0} Json path: {1} was found, but contained no data.", COLLECT_ERROR, path));

            try
            {
                Profile profile = JsonSerializer.Deserialize<Profile>(jsonText);
                return profile;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("{0} Json path: {1} could not be deserialized into a profile.", COLLECT_ERROR, path), ex);
            }
        }

        public string GetImagePath(Profile profile)
        {
            const string COLLECT_ERROR = "Failed to collect profile image!";
            const string IMAGE_PATH = @"images\profiles";

            if (string.IsNullOrEmpty(profile.ImageName))
                throw new ArgumentException(string.Format("{0} Image name was empty for profile: {1}", COLLECT_ERROR, profile.Name));

            string path = Path.Combine(IMAGE_PATH, profile.ImageName);

            if (!File.Exists(Path.Combine(environment.WebRootPath, path)))
                throw new FileNotFoundException(string.Format("{0} Image name: {1} could not be found for profile: {1}", COLLECT_ERROR, path, profile.Name));

            return path;
        }

        public string GetMailToAddress(Profile profile)
        {
            const string MAIL_TO = "mailto";
            return string.Format("{0}:{1}", MAIL_TO, profile.Email);
        }
    }
}
