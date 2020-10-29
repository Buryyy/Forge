using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Forge.DiscordBot.Extensions
{
    public sealed class ConfigurationPreconditionAttribute : PreconditionAttribute
    {

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var logger = (ILogger<ConfigurationPreconditionAttribute>)map.GetService(typeof(ILogger<ConfigurationPreconditionAttribute>));

            if (!(context is SocketCommandContext ctx))
            {
                return PreconditionResult.FromError("Command must have a socket context");
            }

            if (!(context.User is SocketGuildUser user))
            {
                return PreconditionResult.FromError("This command must be run from a server channel.");
            }

            var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            if (context.User.Id == app.Owner.Id)
            {
                logger.LogWarning($"Command being run by owner: {app.Owner.Username}: {command.Aliases.First()}");
                return PreconditionResult.FromSuccess();
            }
            return PreconditionResult.FromSuccess();
        }
    }
}
