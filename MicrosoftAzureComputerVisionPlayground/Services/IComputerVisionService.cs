
namespace MicrosoftAzureComputerVisionPlayground.Services
{
    public interface IComputerVisionService
    {
        Task<string[]> ReadAsync(string url);
    }
}