using System.Threading.Tasks;

namespace Forge.DiscordBot.Interfaces
{
    public interface IBehaviorService
    {
        Task InstallAsync();
        Task RunAsync();
        Task StopAsync();
    }
}
