using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using MicrosoftAzureComputerVisionPlayground.Configuration;
using Serilog;

namespace MicrosoftAzureComputerVisionPlayground.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly ComputerVisionClient _client;
        private readonly ComputerVision _computerVisionConfiguration;

        public ComputerVisionService(IOptions<ComputerVision> options)
        {
            _computerVisionConfiguration = options.Value;
            _client = Authenticate(_computerVisionConfiguration.Endpoint, _computerVisionConfiguration.Key);
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string[]> ReadAsync(string url)
        {
            Log.Information("Starting to read from url: {0}", url);

            ReadHeaders? readResult;
            
            try
            {
                readResult = await _client.ReadAsync(url);
            }
            catch (Exception ex)
            {
                return new[] { ex.Message };
            }

            if (readResult != null)
            {
                ReadOperationResult operationResult;
                var location = readResult.OperationLocation;
                Guid.TryParse(location.Split("/").LastOrDefault(), out var operationId);

                Log.Information("Starting calls for reading the text result: {0}", url);
                do
                {
                    operationResult = await _client.GetReadResultAsync(operationId);
                }
                while (operationResult.Status == OperationStatusCodes.Running || operationResult.Status == OperationStatusCodes.NotStarted);

                if (operationResult is not null)
                {
                    var textUrlFileResults = operationResult.AnalyzeResult.ReadResults;
                    Log.Information("Text results retrieved successfully for operation: {0}", operationId);

                    return textUrlFileResults.SelectMany(x => x.Lines).Select(x => x.Text).ToArray();
                }
                Log.Error("Text result turns out null for operation id: {0}", operationId);
            }

            Log.Warning("Read result turned null for url: {0}", url);
            return Array.Empty<string>();
        }
    }
}
