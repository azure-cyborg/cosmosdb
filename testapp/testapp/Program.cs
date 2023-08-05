using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.Cosmos;
using static Microsoft.Azure.Cosmos.Container;

namespace testapp
{
    class Program
    {
       /* static void Main(string[] args)
        {
            connect();
        } */

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Replace these values with your Cosmos DB account settings
            string cosmosEndpointUri = "https://cosmosdbcyborg.documents.azure.com:443/";
            string cosmosPrimaryKey = "gJ66Q7ZdsNcMAOav1C4SkdQ3tDfVUBWsbrqE79IQ6WWwJXau3tEAYA4Ea6syw4jCUzLpA0nzYT7FACDbz36F4g==";
            string databaseId = "cyborgsou112";
            string containerId = "cyborgidcont1223479";
            string partitionKey = "/categoryId";

            CosmosClientOptions options = new CosmosClientOptions()
            {
                AllowBulkExecution = true
            };
            // Create a new CosmosClient instance
            using (CosmosClient cosmosClient = new CosmosClient(cosmosEndpointUri, cosmosPrimaryKey, options))
            {

                IndexingPolicy policy = new IndexingPolicy();
                policy.IndexingMode = IndexingMode.Consistent;
                policy.ExcludedPaths.Add(
                    new ExcludedPath { Path = "/*" });
                policy.IncludedPaths.Add(
                   new IncludedPath { Path = "/name/?" });


                string sql = "SELECT * FROM products p";
                QueryDefinition query = new QueryDefinition
                    (sql);

           
                // Get or create the desired database
                Microsoft.Azure.Cosmos.Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                ContainerProperties optionsC = new ContainerProperties(containerId, partitionKey);
                optionsC.IndexingPolicy = policy;

                // Get or create the desired container within the database
                Container container = await database.CreateContainerIfNotExistsAsync(optionsC);
                //Container container 

                // Perform database operations here, e.g., querying, inserting, updating, deleting, etc.
                // Example: Inserting a new item
                dynamic newItem = new
                {
                    id = Guid.NewGuid().ToString(),
                    name = "John Doe",
                    age = 30
                };

                Product saddle = new Product()
                {
                    id = "hgfdhfedfsdfff",
                    categoryId = "dresswear",
                    name = "Shirt",
                    price = 67.89d,
                   /* tags = new string[]
                    {
                        "tan",
                        "new",
                        "crisp"
                    } */
                };

            

                string id = "hgfdhfedfsdfff";
                string categoryId = "dresswear";
                PartitionKey key = new PartitionKey(categoryId);

                using FeedIterator<Product> feedIterator = container.GetItemQueryIterator<Product>(query);


               PartitionKey key2 = new PartitionKey(Guid.NewGuid().ToString());
                try
                {
                    //ItemResponse<dynamic> response = await container.CreateItemAsync(newItem, new PartitionKey(newItem.id));
                    saddle.name = "saddleId";

                    //await container.CreateItemAsync<Product>(saddle);
                    await container.UpsertItemAsync<Product>(saddle);
                    Console.WriteLine("Item inserted successfully.");
                   // Product saddle1 = await container.ReadItemAsync<Product>(id, key);


                    Product saddle2 = new Product(Guid.NewGuid().ToString(), "sourav", categoryId);
                    Product saddle3 = new Product(Guid.NewGuid().ToString(), "souravroy", categoryId);

                    Product saddle22 = new Product(Guid.NewGuid().ToString(), "souravrtrt", categoryId);
                    Product saddle33 = new Product(Guid.NewGuid().ToString(), "souravroyffef", categoryId);

                    TransactionalBatch batch = container.CreateTransactionalBatch(key)
                        .CreateItem<Product>(saddle22)
                         .CreateItem<Product>(saddle33);

                    using TransactionalBatchResponse response = await batch.ExecuteAsync();

                    //
                    System.Collections.Generic.List<Product> productsToInsert = new Faker<Product>()
                        .StrictMode(true)
                        .RuleFor(o => o.id, f => Guid.NewGuid().ToString())
                        .RuleFor(o => o.name, f => f.Commerce.ProductName())
                        .RuleFor(o => o.price, f => Convert.ToDouble(f.Commerce.Price(max:1000, min:10)))
                        .RuleFor(o => o.categoryId, f => Guid.NewGuid().ToString())
                        .Generate(1000);

                    System.Collections.Generic.List<Task> concurrentTasks = new System.Collections.Generic.List<Task>();

                    foreach (Product product in productsToInsert)
                    {
                        concurrentTasks.Add(container.CreateItemAsync(product, new PartitionKey(product.categoryId)));
                    }

                 await Task.WhenAll(concurrentTasks);


                    Console.WriteLine($"Read item successfully. {saddle.id} {saddle.name} {saddle.price}");
                    Console.WriteLine($"RTransaction wrote uccessfully. {response.StatusCode}");


                    // Get the results
                    while (feedIterator.HasMoreResults)
                    {
                        FeedResponse<Product> responseP = await feedIterator.ReadNextAsync();

                        foreach (Product product in responseP)
                        {
                            Console.WriteLine("Id {0} ", product.id);
                            Console.WriteLine("Category Id {0} ", product.categoryId);
                            Console.WriteLine("");
                        }

                    }


                    // Change feed
                    Container sourceContainer = cosmosClient.GetContainer(databaseId, "productSource");
                    Container destContainer = cosmosClient.GetContainer(databaseId, "productDest");

                    ChangesHandler<Product> handleChanges = async (
                        IReadOnlyCollection<Product> changes,
                        CancellationToken cancellationToken

                    ) =>
                    {
                        Console.WriteLine($"Start Handling batch of changes...");
                        foreach (Product product in changes)
                        {
                            await Console.Out.WriteLineAsync($"Detected operation {product.id} and {product.name}");
                        }
                    };

                    var builder = sourceContainer.GetChangeFeedProcessorBuilder<Product>(
                        processorName: "produdctsProcessor",
                        onChangesDelegate: handleChanges
                        );

                    ChangeFeedProcessor processor = builder
                        .WithInstanceName("cyborgapp")
                        .WithLeaseContainer(destContainer)
                        .Build();

                    await processor.StartAsync();


                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error while inserting item: {ex.Message}");
                }
            }
        }
    



        static async void connect()
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Hello World!");
            string endpoint = "https://cosmosdbcyborg.documents.azure.com:443/";
            string key = "Zqrs06qyLFRhSlXpqhdBwCScxFHFiW0jlLCK5WGfDMlP62xx93cjrkkXYtX86nHy34Ti3zoLoduEACDbljDmmA==";

            CosmosClient client = new CosmosClient(endpoint, key);

            AccountProperties account = await client.ReadAccountAsync();

           Console.WriteLine(account.Id);
            Console.WriteLine("Hello World!");
            //Console.WriteLine(account.ReadableRegions.ToString());
        }
    }
}

