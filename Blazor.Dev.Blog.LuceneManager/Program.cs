using System;
using System.Collections.Generic;

namespace Blazor.Dev.Blog.LuceneIndex.Manager
{
    class Program
    {
        private const string INDEX_DIRECTORY = "-idxdir";
        private const string CATEGORY_DIRECTORY = "-catdir";
        private const string POST_DIRECTORY = "-pstdir";

        private static Dictionary<string, string> arguments;

        public static string indexDirectory;
        public static string categoryDirectory;
        public static string postDirectory;
        
        static void Main(string[] args)
        {
            arguments = BuildArgsDictionary(args);

            try
            {
                //Load args into vars for easy access.
                indexDirectory = arguments[INDEX_DIRECTORY];
                categoryDirectory = arguments[CATEGORY_DIRECTORY];
                postDirectory = arguments[POST_DIRECTORY];

                CommandInterpreter.PrintInstructions();
                while(true)
                    CommandInterpreter.Interpret(Console.ReadLine());
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        private static Dictionary<string, string> BuildArgsDictionary(string[] args)
        {
            Dictionary<string, string> argPairs = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i += 2)
            {
                argPairs.Add(args[i], args[i + 1]);
            }

            return argPairs;
        }
    }
}
