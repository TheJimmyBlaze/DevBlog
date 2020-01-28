using Blazor.Dev.Blog.LuceneIndex;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blazor.Dev.Blog.Services
{
    public class LuceneSearchService
    {
        private const string INDEX_DIRECTORY = @"data\indexes";

        private readonly IWebHostEnvironment environment;

        public LuceneSearchService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public List<string> SearchBody(string searchTerm)
        {
            List<string> searchTerms = searchTerm.Split(" ").ToList();

            string path = Path.Combine(environment.WebRootPath, INDEX_DIRECTORY);
            Indexer indexer = new Indexer(path);
            List<string> results = indexer.SearchBody(searchTerms);

            return results;
        }

        public List<string> SearchTags(string tag)
        {
            string path = Path.Combine(environment.WebRootPath, INDEX_DIRECTORY);
            Indexer indexer = new Indexer(path);
            List<string> results = indexer.SearchTags(tag);

            return results;
        }

        public List<string> SearchCategories(string category)
        {
            string path = Path.Combine(environment.WebRootPath, INDEX_DIRECTORY);
            Indexer indexer = new Indexer(path);
            List<string> results = indexer.SearchCategories(category);

            return results;
        }
    }
}
