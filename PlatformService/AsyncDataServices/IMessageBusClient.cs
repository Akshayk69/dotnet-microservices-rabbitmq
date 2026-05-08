using PlatformService.Dtos;
using System.Threading.Tasks;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto);
        Task InitializeAsync();   // for async setup
        ValueTask DisposeAsync(); // for graceful cleanup
    }
}
