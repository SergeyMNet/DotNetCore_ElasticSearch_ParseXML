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
    public interface IElasticSearch
    {
        string SaveObject(string filePath);

        List<BookBase> Search(SearchModel model);

        long GetCount();
        List<BookBase> GetAll(int from, int size);
        BookBase GetById(string id);

        Task<string> Delete(string id);

        List<string> GetAllTypes();
        List<string> GetAllFields(string category);


    }

    public class ElasticSearch : IElasticSearch
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
        public List<BookBase> Search(SearchModel model)
        {
            List<BookBase> result = new List<BookBase>();

            try
            {
                var request = CreateSearchRequest(model);

                var res = _client.Search<ModelES>(request);


                foreach (var hit in res.Hits)
                {
                    result.Add(new BookBase(hit.Source));
                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }
        private static SearchRequest CreateSearchRequest(SearchModel q)
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
            QueryContainer qcValue = new QueryContainer();
            //string value = "nodeProps/value";
            string value = "_all";
            
            qcValue = new TermQuery() { Field = value, Value = q.SearchQuery.ToLower() };
            mustquerys.Add(qcValue);

            if (q.Field != "all")
            {
                QueryContainer qcTitle = new QueryContainer();
                //string title = "title";
                string title = "_all";
                qcTitle = new TermQuery() {Field = title, Value = q.Field.ToLower() };
                mustquerys.Add(qcTitle);
            }

            if (q.Category != "all")
            {
                QueryContainer qcCategory = new QueryContainer();
                string category = "category";
                qcCategory = new TermQuery() { Field = category, Value = q.Category.ToLower() };
                mustquerys.Add(qcCategory);
            }

            // ALL quries
            boolQuery = new BoolQuery() { Must = mustquerys, Should = shouldquerys, MustNot = notquerys };

            var r = new SearchRequest()
            {
                Size = 100,
                MinScore = 0.5,
                Query = boolQuery,
            };
            return r;
        }
        #endregion

        #region GetAll

        public long GetCount()
        {
            return _client.Count<ModelES>(new CountRequest("test")).Count;
        }

        public List<BookBase> GetAll(int from, int size)
        {
            List<BookBase> result = new List<BookBase>();

            try
            {
                
                _client.Refresh(new RefreshRequest("test"));
                
                var scanResults = _client.Search<ModelES>(
                    new SearchRequest("test")
                    {
                        From = from,
                        Size = size,
                    }
                );

                foreach (var hit in scanResults.Hits)
                {
                    result.Add(new BookBase(hit.Source));
                }
            }
            catch (Exception ex)
            {

            }


            return result;
        }

        #endregion

        #region GetByID

        public BookBase GetById(string id)
        {
            BookBase result = null;

            try
            {

                var request = new GetRequest("test", "modeles", id);
                var scanResult = _client.GetAsync<ModelES>(request).Result;

                result = new BookBase(scanResult.Source);
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        #endregion

        #region Delete

        public async Task<string> Delete(string id)
        {
            string result = _client.Delete(new DeleteRequest("test", "modeles", id)).Result.ToString();
            
            return result;
        } 
        #endregion


        public List<string> GetAllTypes()
        {
            var categories = new List<string>();

            categories.Add("all");
            var scanResults = _client.Search<ModelES>(
                    new SearchRequest("test")
                    {
                        From = 0,
                        Size = 1000,
                    }
                );

            foreach (var hit in scanResults.Hits)
            {
                
                categories.Add(hit.Source.Category);
            }

            var categoriesSorted = categories.Distinct();

            return categoriesSorted.ToList();
        }

        public List<string> GetAllFields(string category)
        {
            var fields = new List<string>();

            var scanResults = _client.Search<ModelES>(
                    new SearchRequest("test")
                    {
                        From = 0,
                        Size = 1000,
                    }
                );

            fields.Add("all");
            foreach (var hit in scanResults.Hits)
            {
                if(hit.Source.Category == category)
                    foreach (var nodeProp in hit.Source.NodeProps)
                    {
                        if (nodeProp.NodeProps.Any())
                        {
                            foreach (var nodes in nodeProp.NodeProps)
                            {
                                fields.Add(nodes.Title);
                            }
                        }

                        //fields.Add(nodeProp.Title);
                    }
            }

            var fieldsSorted = fields.Distinct();

            return fieldsSorted.ToList();
        }
    }
}
