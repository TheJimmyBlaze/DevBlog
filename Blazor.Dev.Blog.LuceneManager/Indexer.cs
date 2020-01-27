using Blazor.Dev.Blog.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Text;
using static Lucene.Net.Index.IndexWriter;

namespace Blazor.Dev.Blog.LuceneManager
{
    class Indexer
    {
        private const string CATEGORY_NAME = "CategoryName";
        private const string POST_NAME = "PostName";
        private const string POST_ID = "PostID";
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
            document.Add(new Field(CATEGORY_NAME, category.Name, Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(POST_NAME, post.Title, Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(POST_ID, post.PostNaturalID, Field.Store.YES, Field.Index.NO));

            //Add in indexed fields.
            document.Add(new Field(BLURB, post.Blurb, Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(BODY, body, Field.Store.YES, Field.Index.ANALYZED));

            //Combine the Tags into a single string of space separated tags.
            document.Add(new Field(TAGS, string.Join(" ", post.Tags), Field.Store.YES, Field.Index.ANALYZED));

            using IndexWriter writer = CreateIndex(indexDirectory);
            writer.AddDocument(document);
            writer.Flush(false, false, false);
        }

        public void Search(List<string> terms)
        {
            using IndexWriter writer = CreateIndex(indexDirectory);

            MultiPhraseQuery query = new MultiPhraseQuery();
            foreach (string term in terms)
                query.Add(new Term(BODY, term));

            IndexSearcher searcher = new IndexSearcher(writer.GetReader());
            ScoreDoc[] hits = searcher.Search(query, 100).ScoreDocs;

            Console.WriteLine("{0} results...", hits.Length);
            foreach (ScoreDoc hit in hits)
            {
                Console.WriteLine("{0}: {1}", searcher.Doc(hit.Doc).Get(POST_ID), hit.Score);
            }
        }
    }
}
