using Blazor.Dev.Blog.Models;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blazor.Dev.Blog.LuceneManager
{
    static class CommandInterpreter
    {
        public static void PrintInstructions()
        {
            ConsoleColor defaultForeground = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Blazor.Dev.Blog - Lucene Manger - v{0}", typeof(Program).Assembly.GetName().Version);
            Console.ForegroundColor = defaultForeground;

            Console.WriteLine("Commands:");
            Console.WriteLine("Rebuild - Rebuilds the entirety of the post indexes.");
            Console.WriteLine("Test <search_term> - Executes a test search using the provided search term.");
            Console.WriteLine("Help - Reprints these instruction.");
        }

        public static void Interpret(string command)
        {
            string[] args = command.ToLower().Split(" ");

            switch (args[0])
            {
                case "rebuild":
                    Rebuild();
                    break;
                case "test":
                    Test(args.Skip(1).ToList());
                    break;
                case "help":

                    break;
                default:
                    Console.WriteLine("Invalid Command");
                    break;
            }
        }

        private static async void Rebuild()
        {
            Console.WriteLine("Creating indexer in directory: {0}", Program.indexDirectory);
            Indexer indexer = new Indexer(Program.indexDirectory);

            Console.WriteLine("Creating data loader in with directories: {0}, {1}", Program.categoryDirectory, Program.postDirectory);
            DataLoader loader = new DataLoader(Program.categoryDirectory, Program.postDirectory);

            Console.WriteLine("Loading...");
            List<Category> categories = await loader.LoadCategories();
            List<Post> posts = await loader.LoadPosts();

            Console.WriteLine("Indexing...");
            foreach(Post post in posts)
            {
                Category category = categories.SingleOrDefault(category => category.CategoryNaturalID == post.CategoryNaturalID);
                if (category == null)
                    throw new InvalidOperationException(string.Format("Category: {0} not found", post.CategoryNaturalID));

                string body = await loader.GetPostBodyAsync(post);

                indexer.AddToIndex(category, post, body);
            }

            Console.WriteLine("Done");
        }

        private static void Test(List<string> terms)
        {
            Indexer indexer = new Indexer(Program.indexDirectory);
            indexer.Search(terms);
        }
    }
}
