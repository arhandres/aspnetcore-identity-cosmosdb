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
        private CosmosConfiguration _configuration = null;

        private Lazy<DocumentClient> _documentClient = null;
        private Lazy<DocumentCollection> _documentCollection = null;

        protected DocumentClient Context
        {
            get
            {
                return _documentClient?.Value;
            }
        }

        protected DocumentCollection Collection
        {
            get
            {
                return _documentCollection?.Value;
            }
        }

        public BaseRepositoryCosmos(IOptions<CosmosConfiguration> options)
        {
            _configuration = options.Value;

            _documentClient = new Lazy<DocumentClient>(() => this.CreateDocumentClient());
            _documentCollection = new Lazy<DocumentCollection>(() => AsyncHelpers.RunSync<DocumentCollection>(() => this.CreateDocumentCollection()));
        }

        private async Task<DocumentCollection> CreateDocumentCollection()
        {
            var name = typeof(T).Name;
            var databaseUri = UriFactory.CreateDatabaseUri(_configuration.DatabaseName);

            var collection = new DocumentCollection();
            collection.Id = name;
            collection.PartitionKey.Paths.Add("/Partition");

            var result = await this.Context.CreateDocumentCollectionIfNotExistsAsync(databaseUri, collection);

            return collection;
        }

        private DocumentClient CreateDocumentClient()
        {
            var client = new DocumentClient(new Uri(_configuration.EndpointUri), _configuration.PrimaryKey, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            AsyncHelpers.RunSync(() => this.CreateDatabase(client));

            return client;
        }

        protected async Task<ResourceResponse<Database>> CreateDatabase(DocumentClient client)
        {
            var response = await client.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database()
            {
                Id = _configuration.DatabaseName
            });

            return response;
        }

        protected override T GetFirstOrDefault(Func<T, bool> predicate)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_configuration.DatabaseName, this.Collection.Id);

            var item = this.Context.CreateDocumentQuery<T>(uri)
                    .Where(predicate)
                    .AsEnumerable()
                    .FirstOrDefault();

            return item;
        }

        protected override List<T> GetAll(Func<T, bool> predicate = null)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_configuration.DatabaseName, this.Collection.Id);

            var query = this.Context.CreateDocumentQuery<T>(uri);

            if (predicate != null)
                return query.Where(predicate)
                    .AsEnumerable()
                    .ToList();

            return query.AsEnumerable()
                    .ToList();
        }

        protected override async Task<bool> Upsert(T entity)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(_configuration.DatabaseName, this.Collection.Id);
            //var uri = UriFactory.CreateDocumentUri(_configuration.DatabaseName, this.Collection.Id,GetIdPropertyValue(entity));

            var result = await this.Context.UpsertDocumentAsync(uri, entity);

            var success = result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.Created;

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
            var uri = UriFactory.CreateDocumentUri(_configuration.DatabaseName, this.Collection.Id, id);

            var result = await this.Context.DeleteDocumentAsync(uri);

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

    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        { }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        { }
    }
}
