using Blazor.Dev.Blog.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Lucene.Net.Index.IndexWriter;

namespace Blazor.Dev.Blog.LuceneIndex
{
    public class Indexer
    {
        private const int MAX_RESULTS = 100;

        private const string CATEGORY_NAME = "CategoryName";
        private const string POST_NAME = "PostName";
        private const string POST_ID = "PostID";
        private const string POST_DATE = "PostDate";
        private const string BLURB = "Blurb";
        private const string BODY = "Body";
        private const string TAGS = "Tags";

        private readonly Lucene.Net.Util.Version version = Lucene.Net.Util.Version.LUCENE_30;

        private readonly string indexDirectory;

        public Indexer(string indexDirectory)
        {
            this.indexDirectory = indexDirectory;
        }

        private IndexWriter CreateIndex(string indexDirectory)
        {
            FSDirectory store = FSDirectory.Open(indexDirectory);

            Analyzer analyzer = new StopAnalyzer(version);
            IndexWriter writer = new IndexWriter(store, analyzer, MaxFieldLength.UNLIMITED);

            return writer;
        }

        public void AddToIndex(Category category, Post post, string body)
        {
            Document document = new Document();

            //Add in non analyzed fields, ID is not searchable.
            document.Add(new Field(CATEGORY_NAME, Regex.Replace(category.Name, @"[^A-z0-9]", "").ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(POST_NAME, Regex.Replace(post.Title, @"[^A-z0-9]", "").ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            document.Add(new Field(POST_ID, post.PostNaturalID, Field.Store.YES, Field.Index.NO));
            document.Add(new Field(POST_DATE, post.CreationDateUTC.ToString(), Field.Store.YES, Field.Index.NO));

            //Add in indexed fields.
            document.Add(new Field(BLURB, post.Blurb.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(BODY, body.ToLower(), Field.Store.YES, Field.Index.ANALYZED));

            //Combine the Tags into a single string of space separated tags.
            IEnumerable<string> tags = post.Tags.Select(tag => Regex.Replace(tag.Split(";")[1], @"[^A-z0-9]", ""));
            document.Add(new Field(TAGS, string.Join(" ", tags).ToLower(), Field.Store.YES, Field.Index.ANALYZED));

            using IndexWriter writer = CreateIndex(indexDirectory);
            writer.AddDocument(document);
            writer.Flush(false, false, false);
        }

        public List<string> SearchBody(List<string> terms)
        {
            using IndexWriter writer = CreateIndex(indexDirectory);

            MultiPhraseQuery query = new MultiPhraseQuery();
            foreach (string term in terms)
                query.Add(new Term(BODY, term.ToLower()));

            IndexSearcher searcher = new IndexSearcher(writer.GetReader());
            ScoreDoc[] hits = searcher.Search(query, MAX_RESULTS).ScoreDocs;

            List<string> results = hits.Select(hit => searcher.Doc(hit.Doc).Get(POST_ID)).ToList();
            return results;
        }

        public List<string> SearchTags(string tag)
        {
            using IndexWriter writer = CreateIndex(indexDirectory);

            PhraseQuery query = new PhraseQuery();
            query.Add(new Term(TAGS, Regex.Replace(tag, @"[^A-z0-9]", "").ToLower()));

            IndexSearcher searcher = new IndexSearcher(writer.GetReader());
            ScoreDoc[] hits = searcher.Search(query, MAX_RESULTS).ScoreDocs;

            List<string> results = hits.Select(hit => searcher.Doc(hit.Doc).Get(POST_ID)).ToList();
            return results;
        }

        public List<string> SearchCategories(string category)
        {
            using IndexWriter writer = CreateIndex(indexDirectory);

            PhraseQuery query = new PhraseQuery();
            query.Add(new Term(CATEGORY_NAME, Regex.Replace(category, @"[^A-z0-9]", "").ToLower()));

            IndexSearcher searcher = new IndexSearcher(writer.GetReader());
            ScoreDoc[] hits = searcher.Search(query, MAX_RESULTS).ScoreDocs;

            IEnumerable<Document> documents = hits.Select(hit => searcher.Doc(hit.Doc));
            documents = documents.OrderBy(doc => DateTime.Parse(doc.Get(POST_DATE)));

            List<string> results = documents.Select(doc => doc.Get(POST_ID)).ToList();
            return results;
        }
    }
}
