using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auth.BackgroundService
{
    public class ChunkedDataPipeline : IHostedService
    {
        private readonly Channel<List<string>> _channel = Channel.CreateUnbounded<List<string>>();
        private readonly ILogger<ChunkedDataPipeline> logger;
        private const int BatchSize = 10;

        public ChunkedDataPipeline(ILogger<ChunkedDataPipeline> logger)
        {
            this.logger = logger;
        }

        private async Task FetchDataInChunksAsync()
        {
            var allData = Enumerable.Range(1, 50).Select(i => $"Item-{i}").ToList();

            try
            {
                for (int i = 0; i < allData.Count; i += BatchSize)
                {
                    for (int j = 0; j < BatchSize && i + j < allData.Count; j++)
                    {
                        Console.WriteLine($"Importing data..");
                        await Task.Delay(500);
                    }
                    var chunk = allData.Skip(i).Take(BatchSize).ToList();

                    Console.WriteLine($"[Producer] Fetched chunk: {string.Join(", ", chunk)}");

                    await _channel.Writer.WriteAsync(chunk); // send chunk for saving
                    throw new Exception("Simulated exception after completion"); // Simulate an error for testing
                }
            }
            catch (Exception)
            {
                logger.LogError("[Producer] Finished fetching all data.");
                throw new Exception("Process failed"); // Simulate an error for testing
            }
            finally
            {
                _channel.Writer.Complete();
            }
        }

        private async Task SaveChunksToDatabaseAsync()
        {
            await foreach (var chunk in _channel.Reader.ReadAllAsync())
            {
                // Save each chunk in a new parallel task
                _ = Task.Run(async () =>
                {
                    foreach (var item in chunk)
                    {
                        logger.LogInformation($"[Consumer] Processing item: {item}");
                        await Task.Delay(400); // simulate processing delay
                        logger.LogInformation($"[Consumer] Saved chunk.: {item}");
                    }
                });
            }

            logger.LogInformation("[Consumer] All chunks scheduled for saving.");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var producer = FetchDataInChunksAsync();
            var consumer = SaveChunksToDatabaseAsync();

            await Task.WhenAll(producer, consumer);
            logger.LogInformation("ChunkedDataPipeline service started and processing data in chunks.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping ChunkedDataPipeline service.");
            return Task.CompletedTask;
        }
    }

}
