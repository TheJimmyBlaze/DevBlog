using Dev.Blog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dev.Blog.Services
{
    public class ProfileService
    {
        public async Task<Profile> GetProfileAsync(string profileJsonPath)
        {
            const string COLLECT_ERROR = "Failed to collect profile!";
            const string PROFILE_PATH = @"wwwroot/data/profiles";

            string path = Path.Combine(PROFILE_PATH, profileJsonPath);
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
                throw new InvalidOperationException(string.Format("{0} Json path: {1} could not be deserialized into a Profile.", COLLECT_ERROR, path), ex);
            }
        }
    }
}
