using System.Threading.Tasks;

namespace Forge.DiscordBot.Interfaces
{
    public interface IBotBehavior
    {
        string Name { get; }

        Task RunAsync();
        Task StopAsync();
    }
}
