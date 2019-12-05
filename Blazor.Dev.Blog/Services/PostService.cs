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
            const string COLLECT_ERROR = "Failed to collect posts from category!";
            const string POST_PATH = @"data\posts";

            if (postCache != null && !refreshCache)
                return postCache;

            string path = Path.Combine(environment.WebRootPath, POST_PATH);
            if (!Directory.Exists(path))
                throw new FileNotFoundException(string.Format("{0} Post container directory: {1} coult not be found.", COLLECT_ERROR, path));

            //First get all files in the directory.
            List<string> postFiles = Directory.GetFiles(path).ToList();
            List<Post> posts = new List<Post>();

            //Then scan sub directories, and get those files too.
            string[] subDirectories = Directory.GetDirectories(path);
            foreach(string directory in subDirectories)
            {
                postFiles.AddRange(Directory.GetFiles(directory));
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
            return posts.SingleOrDefault(x => x.PostNaturalID == postNaturalID);
        }

        public async Task<IEnumerable<Post>> GetAllForsForCategory(string categoryNaturalID)
        {
            const string COLLECT_ERROR = "Failed to collect posts for category!";

            IEnumerable<Category> categories = await categoryService.GetAllCategoriesAsync();
            Category category = categories.SingleOrDefault(x => x.CategoryNaturalID == categoryNaturalID);

            if (category == null)
                throw new ArgumentException(string.Format("{0} Category: {1} could not be found.", COLLECT_ERROR, categoryNaturalID));

            IEnumerable<Post> posts = await GetAllPostsAync();
            return posts.Where(x => category.PostsNaturalIDs.Contains(x.PostNaturalID));
        }

        public string GetImagePath(Post post)
        {
            const string COLLECT_ERROR = "Failed to collect post image!";
            const string IMAGE_PATH = @"images\posts";

            if (string.IsNullOrEmpty(post.ImageName))
                throw new ArgumentException(string.Format("{0} Image name was empty for post: {1}", COLLECT_ERROR, post.Title));

            string path = Path.Combine(IMAGE_PATH, post.ImageName);

            if (!File.Exists(Path.Combine(environment.WebRootPath, path)))
                throw new FileNotFoundException(string.Format("{0} Image name: {1} could not be found for post: {1}", COLLECT_ERROR, path, post.Title));

            return path;
        }
    }
}
