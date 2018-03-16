using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.DataAccess.Core
{
    public abstract class BaseRepositoryCosmos<T> : BaseRepository<T> where T : class
    {
        private string _databaseName = null;

        private DocumentClient _documentClient = null;
        private DocumentCollection _documentCollection = null;

        protected DocumentClient Context
        {
            get
            {
                return _documentClient;
            }
        }

        public BaseRepositoryCosmos(IOptions<CosmosConfiguration> options)
        {
            var configuration = options.Value;

            _databaseName = configuration.DatabaseName;

            _documentClient = new DocumentClient(new Uri(configuration.EndpointUri), configuration.PrimaryKey, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            this.CreateDatabase();
            this.CreateCollection();
        }

        protected ResourceResponse<Database> CreateDatabase()
        {
            var response = _documentClient.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database()
            {
                Id = _databaseName
            }).Result;

            return response;
        }

        protected void CreateCollection()
        {
            var name = typeof(T).Name;
            var databaseUri = UriFactory.CreateDatabaseUri(_databaseName);

            _documentCollection = new DocumentCollection();
            _documentCollection.Id = name;

            _documentClient.CreateDocumentCollectionAsync(databaseUri, _documentCollection);
        }

        protected override T GetFirstOrDefault(Func<T, bool> predicate)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseName, _documentCollection.Id);

            var item = _documentClient.CreateDocumentQuery<T>(uri)
                    .Where(predicate)
                    .AsEnumerable()
                    .FirstOrDefault();

            return item;
        }

        protected override List<T> GetAll(Func<T, bool> predicate = null)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseName, _documentCollection.Id);

            var query = _documentClient.CreateDocumentQuery<T>(uri);

            if (predicate != null)
                return query.Where(predicate)
                    .AsEnumerable()
                    .ToList();

            return query.AsEnumerable()
                    .ToList();
        }

        protected override async Task<bool> Upsert(T entity)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseName, _documentCollection.Id);

            var result = await _documentClient.UpsertDocumentAsync(uri, entity);

            var success = result.StatusCode == System.Net.HttpStatusCode.Created;

            return success;
        }

        protected override async Task<bool> Delete(T entity)
        {
            var id = this.GetIdPropertyValue(entity);

            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException();

            var success = await this.Delete(id);

            return success;
        }

        protected async Task<bool> Delete(string id)
        {
            var uri = UriFactory.CreateDocumentUri(_databaseName, _documentCollection.Id, id);

            var result = await _documentClient.DeleteDocumentAsync(uri);

            var success = result.StatusCode == System.Net.HttpStatusCode.Created;

            return success;
        }

        private string GetIdPropertyValue(T entity)
        {
            var properties = typeof(T).GetProperties();

            var withJsonAttribute = properties.FirstOrDefault(p =>
            {
                var attribute = Attribute.GetCustomAttribute(p, typeof(JsonPropertyAttribute));
                return attribute != null && ((JsonPropertyAttribute)attribute).PropertyName == "id";
            });

            if (withJsonAttribute != null)
                return Convert.ToString(withJsonAttribute.GetValue(entity));

            var withName = properties.FirstOrDefault(p => string.Compare(p.Name, "Id", true) == 0);

            if (withName != null)
                return Convert.ToString(withName.GetValue(entity));

            return null;
        }
    }

    public class CosmosConfiguration
    {
        public string EndpointUri { get; set; }

        public string PrimaryKey { get; set; }

        public string DatabaseName { get; set; }
    }
}
