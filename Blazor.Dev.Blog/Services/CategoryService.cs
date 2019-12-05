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
    public class CategoryService
    {
        private IEnumerable<Category> categoryCache;

        private readonly IWebHostEnvironment environment;
        public CategoryService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(bool refreshCache = false)
        {
            const string COLLECT_ERROR = "Failed to collect categories!";
            const string CATEGORY_PATH = @"data\categories";

            if (categoryCache != null && !refreshCache)
                return categoryCache;

            string path = Path.Combine(environment.WebRootPath, CATEGORY_PATH);
            if (!Directory.Exists(path))
                throw new FileNotFoundException(string.Format("{0} Category container directory: {1} could not be found.", COLLECT_ERROR, path));

            string[] categoryFiles = Directory.GetFiles(path);
            List<Category> categories = new List<Category>();

            foreach(string file in categoryFiles)
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(stream);
                string jsonText = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(jsonText))
                    throw new FileLoadException(string.Format("{0} Json path: {1} was found, but contained no data.", COLLECT_ERROR, file));

                try
                {
                    Category category = JsonSerializer.Deserialize<Category>(jsonText);
                    categories.Add(category);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("{0} json path: {1} could not be deserializede into a category.", COLLECT_ERROR, path), ex);
                }
            }

            categoryCache = categories;
            return categoryCache;
        }

        public async Task<Category> GetCategoryOfPost(string postNaturalID)
        {
            IEnumerable<Category> categories = await GetAllCategoriesAsync();
            return categories.SingleOrDefault(x => x.PostsNaturalIDs.Contains(postNaturalID));
        }

        public string GetImagePath(Category category)
        {
            const string COLLECT_ERROR = "Failed to collect category image!";
            const string IMAGE_PATH = @"images\categories";

            if (string.IsNullOrEmpty(category.ImageName))
                throw new ArgumentException(string.Format("{0} Image name was empty for category: {1}", COLLECT_ERROR, category.Name));

            string path = Path.Combine(IMAGE_PATH, category.ImageName);

            if (!File.Exists(Path.Combine(environment.WebRootPath, path)))
                throw new FileNotFoundException(string.Format("{0} Image name: {1} could not be found for category: {1}", COLLECT_ERROR, path, category.Name));

            return path;
        }
    }
}
