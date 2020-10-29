using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;

namespace Forge.DiscordBot.Models
{
    public class DiscordLogger<T>
    {
        private ILogger<T> _logger;
        public DiscordLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public Task OnLogAsync(LogMessage msg)
        {
            var message = msg.ToString();

            switch (msg.Severity)
            {
                case LogSeverity.Info:
                    _logger.LogInformation(message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    _logger.LogError(msg.Exception != null ? msg.Exception.Message : message);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.Exception != null ? msg.Exception.Message : message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
