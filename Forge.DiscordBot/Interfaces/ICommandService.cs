using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Forge.DiscordBot.Models;

namespace Forge.DiscordBot.Interfaces
{
  public interface ICommandService
  {
      Task InstallAsync();
      Task ReloadCommands();
      Task<List<Help>> BuildHelpAsync(ICommandContext context);
    }
}
