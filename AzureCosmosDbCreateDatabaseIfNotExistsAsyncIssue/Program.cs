using System.Diagnostics;
using Microsoft.Azure.Cosmos;

const string databaseId = "Test";
const string accountEndpoint = "https://localhost:8081/";
//const string accountEndpoint = "https://host.docker.internal:8081/";
const string authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

var cosmosClient = new CosmosClient(accountEndpoint, authKey, new CosmosClientOptions
{
    HttpClientFactory = () =>
    {
        HttpMessageHandler httpMessageHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return new HttpClient(httpMessageHandler);
    },
    ConnectionMode = ConnectionMode.Gateway,
});

const string containerId = "TestContainer";
const string partitionKeyPath = "/path";
const int requestsCount = 10;

var stopwatch = Stopwatch.StartNew();
var tasks = new List<Task>();
for (var i = 0; i < requestsCount; i++)
{
    tasks.Add(EnsureContainerExistsAsync());
}

try
{
    await Task.WhenAll(tasks);
}
catch (Exception e) when (e is CosmosException ce)
{
    Console.WriteLine($"Status code: {ce.StatusCode}. {ce.Message}.");
}
finally
{
    stopwatch.Stop();
    Console.WriteLine($"ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}.");
}

Console.ReadLine();

async Task EnsureContainerExistsAsync()
{
    var response = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
    await response.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, partitionKeyPath));
}