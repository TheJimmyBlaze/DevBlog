using Blazor.Dev.Blog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Blazor.Dev.Blog.LuceneIndex.Manager
{
    class DataLoader
    {
        private const string JSON = "*.json";

        private readonly string categoryDirectory;
        private readonly string postDirectory;

        public DataLoader(string categoryDirectory, string postDirectory)
        {
            this.categoryDirectory = categoryDirectory;
            this.postDirectory = postDirectory;
        }

        public async Task<List<Category>> LoadCategories()
        {
            if (!Directory.Exists(categoryDirectory))
                throw new FileNotFoundException(string.Format("Category folder: {0} not found", categoryDirectory));

            string[] categoryFiles = Directory.GetFiles(categoryDirectory, JSON);
            List<Category> categories = new List<Category>();

            foreach(string file in categoryFiles)
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(stream);
                string jsonText = await reader.ReadToEndAsync();

                Category category = JsonSerializer.Deserialize<Category>(jsonText);
                categories.Add(category);
            }

            return categories;
        }

        public async Task<List<Post>> LoadPosts()
        {
            if (!Directory.Exists(postDirectory))
                throw new FileNotFoundException(string.Format("Post folder: {0} not found", postDirectory));

            List<string> postFiles = Directory.GetFiles(postDirectory, JSON).ToList();
            List<Post> posts = new List<Post>();

            string[] subDirectories = Directory.GetDirectories(postDirectory);
            foreach(string directory in subDirectories)
            {
                postFiles.AddRange(Directory.GetFiles(directory, JSON));
            }

            foreach(string file in postFiles)
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(stream);
                string jsonText = await reader.ReadToEndAsync();

                Post post = JsonSerializer.Deserialize<Post>(jsonText);
                posts.Add(post);
            }

            return posts;
        }

        public async Task<string> GetPostBodyAsync(Post post)
        {
            string path = Path.Combine(postDirectory, post.Body);
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("Post body: {0} not found", path));

            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(stream);
            string bodyText = await reader.ReadToEndAsync();

            return bodyText;
        }
    }
}
