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
    public class PostService
    {
        private IEnumerable<Post> postCache;

        private readonly IWebHostEnvironment environment;
        private readonly CategoryService categoryService;
        public PostService(IWebHostEnvironment environment, CategoryService categoryService)
        {
            this.environment = environment;
            this.categoryService = categoryService;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAync(bool refreshCache = false)
        {
            const string COLLECT_ERROR = "Failed to collect posts!";
            const string POST_PATH = @"data\posts";

            if (postCache != null && !refreshCache)
                return postCache;

            string path = Path.Combine(environment.WebRootPath, POST_PATH);
            if (!Directory.Exists(path))
                throw new FileNotFoundException(string.Format("{0} Post container directory: {1} could not be found.", COLLECT_ERROR, path));

            //First get all files in the directory.
            List<string> postFiles = Directory.GetFiles(path, "*.json").ToList();
            List<Post> posts = new List<Post>();

            //Then scan sub directories, and get those files too.
            string[] subDirectories = Directory.GetDirectories(path);
            foreach(string directory in subDirectories)
            {
                postFiles.AddRange(Directory.GetFiles(directory, "*.json"));
            }

            foreach(string file in postFiles)
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(stream);
                string jsonText = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(jsonText))
                    throw new FileLoadException(string.Format("{0} Json path: {1} was found, but contained no data.", COLLECT_ERROR, file));

                try
                {
                    Post post = JsonSerializer.Deserialize<Post>(jsonText);
                    posts.Add(post);
                }
                catch(Exception ex)
                {
                    throw new InvalidOperationException(string.Format("{0} json path: {1} could not be deserialized into a post.", COLLECT_ERROR, path), ex);
                }
            }

            postCache = posts;
            return postCache;
        }

        public async Task<Post> GetPost(string postNaturalID)
        {
            IEnumerable<Post> posts = await GetAllPostsAync();
            return posts.SingleOrDefault(x => x.PostNaturalID.ToLower() == postNaturalID.ToLower());
        }

        public async Task<IEnumerable<Post>> GetAllPostsForCategory(string categoryNaturalID, bool refreshCache = false)
        {
            const string COLLECT_ERROR = "Failed to collect posts for category!";

            IEnumerable<Category> categories = await categoryService.GetAllCategoriesAsync();
            Category category = categories.SingleOrDefault(x => x.CategoryNaturalID == categoryNaturalID);

            if (category == null)
                throw new ArgumentException(string.Format("{0} Category: {1} could not be found.", COLLECT_ERROR, categoryNaturalID));
            
            return (await GetAllPostsAync(refreshCache)).Where(post => post.CategoryNaturalID == categoryNaturalID);
        }

        public async Task<string> GetPostBodyAsync(Post post)
        {
            if (post == null || string.IsNullOrEmpty(post.Body))
                return null;

            const string COLLECT_ERROR = "Failed to collect posts body!";
            const string POST_PATH = @"data\posts";

            string directory = Path.Combine(environment.WebRootPath, POST_PATH);
            if (!Directory.Exists(directory))
                throw new FileNotFoundException(string.Format("{0} Post container directory: {1} could not be found.", COLLECT_ERROR, directory));

            string path = Path.Combine(directory, post.Body);

            if (!File.Exists(Path.Combine(environment.WebRootPath, path)))
                throw new FileNotFoundException(string.Format("{0} Body file: {1} could not be found for post: {1}", COLLECT_ERROR, post.Body, post.PostNaturalID));


            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(stream);
            string bodyText = await reader.ReadToEndAsync();

            return bodyText;
        }

        public string GetImagePath(Post post)
        {
            if (post == null)
                return null;

            const string COLLECT_ERROR = "Failed to collect post image!";
            const string IMAGE_PATH = @"images\posts";

            if (string.IsNullOrEmpty(post.ImageName))
                throw new ArgumentException(string.Format("{0} Image name was empty for post: {1}", COLLECT_ERROR, post.Title));

            string path = Path.Combine(IMAGE_PATH, post.ImageName);

            if (!File.Exists(Path.Combine(environment.WebRootPath, path)))
                throw new FileNotFoundException(string.Format("{0} Image name: {1} could not be found for post: {1}", COLLECT_ERROR, path, post.Title));

            return path;
        }

        public Tuple<string, string> CreateTag(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            const string TAG_ERROR = "Failed to create tag!";

            Dictionary<string, string> types = new Dictionary<string, string>()
            {
                ["category"] = "primary",
                ["topic"] = "info",
                ["tech"] = "secondary",
                ["goodflair"] = "success",
                ["badflair"] = "danger"
            };

            if (!raw.Contains(";"))
                throw new ArgumentException(string.Format("{0} Tag: {1} did not contain a separating ';' character.", TAG_ERROR, raw));

            string[] components = raw.Split(";");
            string type = types[components[0].ToLower()];
            string text = components[1];

            if (string.IsNullOrEmpty(type))
                throw new ArgumentException(string.Format("{0} Tag: {1} did not contain a recognized type.", TAG_ERROR, raw));
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(string.Format("{0} Tag: {1} did not contain any text.", TAG_ERROR, raw));

            return new Tuple<string, string>(type, text);
        }
    }
}
