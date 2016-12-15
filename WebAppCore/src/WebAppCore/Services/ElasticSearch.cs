using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Elasticsearch.Net;
using Nest;
using WebAppCore.Models.BookViewModel;
using WebAppCore.Models.ES_Models;

namespace WebAppCore.Services
{
    public class ElasticSearch
    {
        private readonly string mainPath = @"http://localhost:9200/";
        private readonly ElasticClient _client;

        public ElasticSearch()
        {
            var nodes = new Uri[] { new Uri(mainPath) };
            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool).DisableDirectStreaming();

            _client = new ElasticClient(settings);
        }

        #region Submit
        public string SaveObject(string filePath)
        {
            string result = "";

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                
                // conver XML to model
                var doc = new XmlDocument();
                doc.Load(stream);
                var model = ConverterXml_to_Model.ToModelEs(doc);
                string name = Path.GetFileNameWithoutExtension(filePath);
                model.Title = name;
                model.Category = doc?.LastChild.Name;
                model.UrlStorage = filePath;

                result = SendObject(model, "test", Path.GetFileNameWithoutExtension(filePath));
            }

            return result;
        }

        private string SendObject(ModelES sendObject, string index, string id)
        {
            var response = _client.IndexAsync(sendObject, idx => idx.Index(index.ToLower()).Id(id)).Result;
            return response.Result.ToString();
        } 
        #endregion
        
        #region Search
        public List<string> Search(string q)
        {
            List<string> result = new List<string>();

            try
            {
                var request = CreateSearchRequest(q.ToLower());

                var res = _client.Search<ModelES>(request);
                Console.WriteLine(res.Hits.Count());
                foreach (var hit in res.Hits)
                {
                    //if(hit.Index == "test")
                        result.Add(hit.Id);
                }
            }
            catch (Exception ex)
            {
                
            }
            return result;
        }
        private static SearchRequest CreateSearchRequest(string q)
        {

            //bool
            QueryContainer boolQuery = new BoolQuery() { };
            //must
            List<QueryContainer> mustquerys = new List<QueryContainer>();
            //should
            List<QueryContainer> shouldquerys = new List<QueryContainer>();
            //not
            List<QueryContainer> notquerys = new List<QueryContainer>();


            // create Query
            QueryContainer qc = new QueryContainer();
            string field = "_all";
            qc = new TermQuery() { Field = field, Value = q };
            mustquerys.Add(qc);

            // ALL quries
            boolQuery = new BoolQuery() { Must = mustquerys, Should = shouldquerys, MustNot = notquerys };

            var r = new SearchRequest()
            {
                Size = 100,
                //MinScore = 0.5,
                Query = boolQuery,
            };
            return r;
        }
        #endregion

        #region GetAll

        public List<BookBase> GetAll(int from, int size)
        {
            List<BookBase> result = new List<BookBase>();

            try
            {
                var scanResults = _client.Search<ModelES>(s => s
                .Index("test")
                .From(from)
                .Size(size)
                .MatchAll()
                .Scroll("5m")
                );

                foreach (var hit in scanResults.Hits)
                {
                    //if(hit.Index == "test")
                    result.Add(new BookBase(hit.Source));
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        #endregion
    }
}
