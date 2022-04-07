using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using MicrosoftAzureComputerVisionPlayground.Configuration;
using Serilog;

namespace MicrosoftAzureComputerVisionPlayground.Services
{
    /// <summary>
    /// Azure computer vision interaction service.
    /// Defines helper methods which acts as a wrapper against the computer vision API
    /// </summary>
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly ComputerVisionClient _client;
        private readonly ComputerVision _computerVisionConfiguration;

        public ComputerVisionService(IOptions<ComputerVision> options)
        {
            _computerVisionConfiguration = options.Value;
            _client = BuildVisionClient(_computerVisionConfiguration.Endpoint, _computerVisionConfiguration.Key);
        }

        /// <summary>
        /// Build vision API client which will be used to interact with the API
        /// </summary>
        /// <param name="endpoint">Endpoint url which points to your computer vision resource created within Azure portal</param>
        /// <param name="key">API key used for authentication</param>
        /// <returns>ComputerVisionClient which is used for all the interactions</returns>
        private ComputerVisionClient BuildVisionClient(string endpoint, string key)
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
        }

        /// <summary>
        /// With a given URL of an image, reads the text contents within it.
        /// </summary>
        /// <param name="url">Image url which is stored somewhere within the internet</param>
        /// <returns>A string array containing text within the image</returns>
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
