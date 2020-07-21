using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;

namespace CosmosGettingStartedTutorial
{
    class Program
    {
        // <Main>
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Beginning operations...\n");
                CosmosClient client = new CosmosClient("https://localhost:8081/", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
                Database database = await client.CreateDatabaseIfNotExistsAsync("ToDoList");
                Container container = database.GetContainer("jason");
                // Add for an item
                QueryDefinition queryDefinition = new QueryDefinition("select * from c ");
                FeedIterator<MyEntity> feedIterator = container.GetItemQueryIterator<MyEntity>(queryDefinition, null, new QueryRequestOptions() { PartitionKey = new PartitionKey("address0") });
                int count = 0;
                while (feedIterator.HasMoreResults)
                {
                    foreach (var item in await feedIterator.ReadNextAsync())
                    {
                        item.ttl = 10;
                        await container.UpsertItemAsync<MyEntity>(item, new PartitionKey(item.address));
                        count++;
                    }
                }

                int num = count + 5;
                for (int i = count; i < num; i++)
                {
                    MyEntity entity = new MyEntity(i.ToString(), i.ToString(), i.ToString());
                    entity.id = i.ToString();
                    entity.address = "address0";
                    await container.CreateItemAsync<MyEntity>(entity, new PartitionKey(entity.address));
                }
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }
        public class MyEntity : TableEntity
        {
            public string Prop { get; set; }

            [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
            public int? ttl { get; set; }

            public MyEntity(string pk, string rk, string prop)
            {
                this.PartitionKey = pk;
                this.RowKey = rk;
                this.Prop = prop;
                this.ttl = -1;
            }
            public string address { get; set; }
            public string id { get; set; }
        }
        static void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }
    }
}
